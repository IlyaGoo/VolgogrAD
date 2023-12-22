using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/** Зона включения квеста, требующего активацию
Например, готовка
 */
public class QuestStartAreaDoing : TriggerAreaDoing
{
    private QuestController _questController;
    public override bool NeedShowLabel()
    {
        return CanInteract();
    }

    public void SetQuest(QuestController newQuestController, bool active) {
        if (_questController != null)
        {
            Debug.LogError("Попытка установить квест на QuestStartAreaDoing, у которой уже есть квест");
            return;
        }
        _questController = newQuestController;
        gameObject.SetActive(active);
    }

    public QuestController GetQuest() {
        return _questController;
    }

    /** Возможность взаимодействовать с квестом */
    private bool CanInteract() {
        return PlayerThere && _questController != null && !_questController.started;
    } 

    public override bool Do()
    {
        if (!CanInteract()) return false;
        localCommands.CmdStartQuest(_questController.gameObject);
        gameObject.SetActive(false);
        return true;
    }

    private void Awake(){
        if (!enabled) return;
        //Стагиваем с парента всю информацию по коллайдеру и кнопке начала
        var col = GetComponent<BoxCollider2D>();
        var parent = transform.parent;
        var parentCol = parent.GetComponent<BoxCollider2D>();
        col.offset = parentCol.offset;
        col.size = parentCol.size;

        var doing = GetComponent<QuestStartAreaDoing>();
        var areaData = parent.GetComponent<QuestStartAreaData>();
        doing.labelText = areaData.labelText;
        doing.textOffset = areaData.textOffset;
        doing.labelSize = areaData.labelSize;
    }

    public void Start()
    {
        RequestData();
    }

    [Client]
    private void RequestData()
    {
        if (localCommands.isServer) return;
        localCommands.CmdRequestInitQuestActivator(gameObject);
    }
}