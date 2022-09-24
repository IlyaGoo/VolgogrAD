using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Зона включения квеста, требующего активацию
Например, готовка
 */
public class QuestStartAreaDoing : TriggerAreaDoing
{
    private QuestController controller;
    public override bool NeedShowLabel()
    {
        return canInteract();
    }

    public void SetQuest(QuestController newQuest) {
        if (controller != null) return;
        controller = newQuest;
    }

    public QuestController GetQuest() {
        return controller;
    }

    /** Возможность взаимодействовать с квестом */
    private bool canInteract() {
        return PlayerThere && controller != null && !controller.started;
    } 

    public override bool Do()
    {
        if (!canInteract()) return false;
        localCommands.CmdStartQuest(gameObject);
        gameObject.SetActive(false);
        return true;
    }

    void Awake(){
        if (!enabled) return;
        //Стагиваем с парента всю информацию по коллайдеру и кнопке начала
        var col = GetComponent<BoxCollider2D>();
        var parentCol = transform.parent.GetComponent<BoxCollider2D>();
        col.offset = parentCol.offset;
        col.size = parentCol.size;

        var doing = GetComponent<QuestStartAreaDoing>();
        var areaData = transform.parent.GetComponent<QuestStartAreaData>();
        doing.labelText = areaData.labelText;
        doing.textOffset = areaData.textOffset;
        doing.labelSize = areaData.labelSize;
    }

    public void Start() {
        localCommands.CmdRequestInitQuestActivator(gameObject);
    }
}