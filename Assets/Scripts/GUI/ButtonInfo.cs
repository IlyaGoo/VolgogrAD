using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInfo : MonoBehaviour {

    [SerializeField] public GameObject arrowPrefab;
    public bool isTarget = false;
    public Transform targetTransform;
    public string text;
    public bool canChange = true;
    public float height;
    public Menu ownMenu;
    public int number;
    public QuestStep gameController = null;
    public GameObject buttonObject;

    public GameObject arrow;

    public void ChangePoint(QuestStep gameController)
    {
        this.gameController = gameController;
    }

    public void ChangeTarget(bool state)
    {
        if (!canChange) return;
        //if (gameController != null) ownMenu.mapScript.SetState(gameController.taskPoint, isTarget ? CircleState.Enable : CircleState.Choosen);
        if (isTarget == state) return;
        isTarget = state;
        if (state)
        {
            ownMenu.targetButton = this;
            TaskMenuScript.instance.targetMenu = ownMenu;
            arrow = Instantiate(arrowPrefab, new Vector3(gameObject.transform.position.x - 25, gameObject.transform.position.y + 8, gameObject.transform.position.z), Quaternion.identity, gameObject.transform);
        }
        else
        {
            ownMenu.targetButton = null;
            TaskMenuScript.instance.targetMenu = null;
            Destroy(arrow);
        }
    }
}
