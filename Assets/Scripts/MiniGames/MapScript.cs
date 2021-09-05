using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScript : MonoBehaviour {
    [SerializeField] GameObject panel;
    GameObject text;
    [SerializeField] GameObject circlePrefab;
    [SerializeField] GameObject circle2Prefab = null;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject textPrefab;
    public GameObject mapGameObject;
    List<List<GameObject>> points = new List<List<GameObject>>();
    List<GameObject> allPoints = new List<GameObject>();
    float y = 15;
    float x = 75;

    TaskControllerScript controllerScript;
    public TaskControllerScript ControllerScript {
        get
        {
            if (controllerScript == null)
                controllerScript = GetComponent<TaskControllerScript>();
            return controllerScript;
        }
    }

    Color enableColor = new Vector4(210/255f, 214 / 255f, 76 / 255f, 1);
    Color choosenColor = new Vector4(214 / 255f, 167 / 255f, 76 / 255f,1);
    Color disableColor = new Vector4(214 / 255f, 83 / 255f, 76 / 255f,1);
    Color endedColor = new Vector4(76 / 255f, 214 / 255f, 86 / 255f,1);

    // Use this for initialization
/*    void Start () {
        mapGameObject = Instantiate(new GameObject(), new Vector3(panel.transform.position.x, panel.transform.position.y, panel.transform.position.z), Quaternion.identity, panel.transform);
        text = Instantiate(textPrefab, new Vector3(mapGameObject.transform .position.x- 15.2f, mapGameObject.transform.position.y+186.6f, mapGameObject.transform.position.z+10), Quaternion.identity, mapGameObject.transform);
        //text.transform.position = new Vector3(-15.2f, 186.6f, 10);
    }
*/
    public void SpawnStartObjects()
    {
        mapGameObject = Instantiate(new GameObject(), new Vector3(panel.transform.position.x, panel.transform.position.y, panel.transform.position.z), Quaternion.identity, panel.transform);
        text = Instantiate(textPrefab, new Vector3(mapGameObject.transform.position.x - 15.2f, mapGameObject.transform.position.y + 186.6f, mapGameObject.transform.position.z + 10), Quaternion.identity, mapGameObject.transform);
    }

    public void CloseMap()
    {
        panel.SetActive(false);
        points.Clear();
        allPoints.Clear();
        Destroy(mapGameObject);
    }

    public void ChangeMainText(string str)
    {
        text.GetComponent<TextMeshProUGUI>().SetText(str);
    }

    public void SetState(GameObject mapCircleObject, CircleState state)
    {
        //if (taskPoint.number > allPoints.Count - 1) return;

        Color newColor;
        switch (state)
        {
            case CircleState.Enable:
                newColor = enableColor;
                break;
            case CircleState.Choosen:
                newColor = choosenColor;
                break;
            case CircleState.Disable:
                newColor = disableColor;
                break;
            case CircleState.Ended:
                newColor = endedColor;
                break;
            default:
                newColor = new Color();
                break;
        }

        
        mapCircleObject.GetComponent<Image>().color = newColor;
    }
    
    public void AddCircle(TaskPoint point, CircleState state = CircleState.Disable)
    {
        /*if (allPoints.Count == 0) SpawnStartObjects();*/
        if (point.level + 1 > points.Count) points.Add(new List<GameObject>());
        var newCircle = Instantiate(circlePrefab, new Vector3(panel.transform.position.x, panel.transform.position.y + 120 - 5 *y * point.level, panel.transform.position.z), Quaternion.identity, mapGameObject.transform);
        points[point.level].Add(newCircle);
        point.mapCircleObject = newCircle;

        var CircleComponent = newCircle.GetComponent<Circle>();
        CircleComponent.taskPoint = point;

        newCircle.GetComponent<Button>().onClick.AddListener(delegate { ControllerScript.ChangeTarget(CircleComponent.taskPoint); });

        for (int i = 0; i < points[point.level].Count; i++)
        {
            points[point.level][i].transform.position = new Vector3(panel.transform.position.x - (points[point.level].Count-1) * x / 2 + i * x, points[point.level][i].transform.position.y, points[point.level][i].transform.position.z);
        }
        allPoints.Add(newCircle);
        SetState(point.mapCircleObject, state);


        if (point.needPointsNums != null)
            foreach (var parent in point.needPointsNums)
            {
                var parentTransform = allPoints[parent].transform;

                float a = Mathf.Abs(parentTransform.position.y - newCircle.transform.position.y);
                float b = Mathf.Abs(parentTransform.position.x - newCircle.transform.position.x);
                float c = Mathf.Sqrt(a * a + b * b);

                float alpha = Mathf.Asin(a/c)/Mathf.PI*180;

                float dopangle = newCircle.transform.position.x > parentTransform.position.x ? newCircle.transform.position.y > parentTransform.position.y ? alpha : 360 - alpha : newCircle.transform.position.y > parentTransform.position.y ? 180 - alpha : 180 + alpha;

                var newLine = Instantiate(linePrefab, new Vector3(parentTransform.position.x, parentTransform.position.y, -1), Quaternion.identity, parentTransform);
                var circle2 = Instantiate(circle2Prefab, new Vector3(parentTransform.position.x, parentTransform.position.y, -1), Quaternion.identity, parentTransform);
                newLine.transform.rotation = Quaternion.Euler(0, 0, dopangle);

                var tr = newLine.GetComponent<RectTransform>();
                tr.sizeDelta = new Vector2(c, 5);
            }
    }
}

public enum CircleState
{
    Choosen, Enable, Disable, Ended
}
