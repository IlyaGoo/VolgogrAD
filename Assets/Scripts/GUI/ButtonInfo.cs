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
    public int numberCostil = -1;
    private int pointNum = -1;
    public GameObject buttonObject;

    public GameObject arrow;

    public void ChangePoint(int num)
    {
        pointNum = num;
    }

    public void ChangeTarget(bool state)
    {
        if (pointNum != -1) ownMenu.mapScript.SetState(pointNum, isTarget ? CircleState.Enable : CircleState.Choosen);
        if (isTarget == state) return;
        if (isTarget)
        {
            isTarget = false;
            Destroy(arrow);
        }
        else
        {
            isTarget = true;
            ownMenu.targetButton = this;
            arrow = Instantiate(arrowPrefab, new Vector3(gameObject.transform.position.x - 25, gameObject.transform.position.y + 8, gameObject.transform.position.z), Quaternion.identity, gameObject.transform);
        }
    }
}
