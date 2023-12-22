using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ButtonInfo : MonoBehaviourExtension {

    [SerializeField] public GameObject arrowPrefab;
    public bool isTarget = false;
    public string text;
    public float height;
    [FormerlySerializedAs("ownMenu")] public QuestButton ownQuestButton;
    public int number;
    public IQuestTarget questStep;
    public GameObject buttonObject;

    public GameObject arrow;

    public void ChangeButtonTarget(IQuestTarget newQuestStep)
    {
        questStep = newQuestStep;
    }
    
    public Transform GetTargetTransform()
    {
        return questStep == null ? null : questStep.GetTargetTransform();
    }

    public void ChangeTarget(bool state)
    {
        //if (gameController != null) ownMenu.mapScript.SetState(gameController.taskPoint, isTarget ? CircleState.Enable : CircleState.Choosen);
        if (isTarget == state) return;
        isTarget = state;
        if (state)
        {
            QuestPanelScript.Instance.targetQuestButton = this;
            arrow = Instantiate(arrowPrefab, new Vector3(gameObject.transform.position.x - 25, transform.position.y + 14, transform.position.z), Quaternion.identity, transform);
        }
        else
        {
            QuestPanelScript.Instance.targetQuestButton = null;
            Destroy(arrow);
        }
    }
}
