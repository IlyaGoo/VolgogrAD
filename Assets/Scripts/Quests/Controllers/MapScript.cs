using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScript : MonoBehaviourExtension, IListener
{
    public static MapScript Instance;
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject circlePrefab;
    [SerializeField] GameObject circle2Prefab = null;
    [SerializeField] GameObject linePrefab;
    public GameObject mapGameObject;
    List<List<Circle>> points = new();
    private List<Circle> allPoints => points.SelectMany(point => point).ToList();
    float y = 15;
    float x = 75;

    void Awake()
    {
        Instance = this;
    }

    private void ChangeState(bool state)
    {
        panel.SetActive(state);
        if (state)
        {
            localListenersManager.TabListeners.Add(this);
            localListenersManager.EscListeners.Add(this);
        }
        else
        {
            points.Clear();
            Destroy(mapGameObject);
            localListenersManager.TabListeners.Remove(this);
            localListenersManager.EscListeners.Remove(this);
        }
    }

    public void EventDid()
    {
        ChangeState(false);
    }

    QuestController currentControllerScript;

    public void OpenWithQuest(QuestController quest)
    {
        if (currentControllerScript == quest)
        {
            currentControllerScript = null;
            ChangeState(false);
        }
        else
        {
            if (currentControllerScript != null)
                ChangeState(false);
            currentControllerScript = quest;
            text.SetText(quest.currentQuestName);
            ChangeState(true);
            BuildMap(quest);
        }
    }

    public void UpdateAllSteps(QuestController quest)
    {
        if (quest == currentControllerScript)
        {
            allPoints.ForEach(point =>
                point.updateState()
            );
        }
    }

    private void BuildMap(QuestController quest)
    {
        mapGameObject = Instantiate(new GameObject(),
            new Vector3(panel.transform.position.x, panel.transform.position.y, panel.transform.position.z),
            Quaternion.identity, panel.transform);

        foreach (var step in quest.allSteps)
        {
            AddCircle(step);
        }

        BuildLines();
    }

    public void CloseMap(QuestController closeQuest = null)
    {
        if (closeQuest == null || closeQuest == currentControllerScript)
        {
            ChangeState(false);
            currentControllerScript = null;
        }
    }

    private void BuildLines()
    {
        foreach (var point in allPoints)
        {
            if (point.level == 0) continue;
            var needPoints = allPoints.FindAll(parentPoint => point.step.needSteps.Contains(parentPoint.step));

            if (needPoints.Count > 0)
                foreach (var parent in needPoints)
                {
                    var parentTransform = parent.transform;

                    var a = Mathf.Abs(parentTransform.position.y - point.transform.position.y);
                    var b = Mathf.Abs(parentTransform.position.x - point.transform.position.x);
                    var c = Mathf.Sqrt(a * a + b * b);

                    var alpha = Mathf.Asin(a / c) / Mathf.PI * 180;

                    var angle = point.transform.position.x > parentTransform.position.x
                        ? point.transform.position.y > parentTransform.position.y ? alpha : 360 - alpha
                        : point.transform.position.y > parentTransform.position.y
                            ? 180 - alpha
                            : 180 + alpha;

                    var newLine = Instantiate(linePrefab,
                        new Vector3(parentTransform.position.x, parentTransform.position.y, -1), Quaternion.identity,
                        parentTransform);
                    var circle2 = Instantiate(circle2Prefab,
                        new Vector3(parentTransform.position.x, parentTransform.position.y, -1), Quaternion.identity,
                        parentTransform);
                    newLine.transform.rotation = Quaternion.Euler(0, 0, angle);

                    var tr = newLine.GetComponent<RectTransform>();
                    tr.sizeDelta = new Vector2(c, 5);
                }
        }
    }

    private void AddCircle(QuestStep step)
    {
        var needPoints = allPoints.FindAll(point => step.needSteps.Contains(point.step));
        var level = needPoints.Count == 0 ? 0 : needPoints.Select(point => point.level).Max() + 1;

        if (level + 1 > points.Count) points.Add(new List<Circle>());
        var newCircle = Instantiate(circlePrefab,
            new Vector3(panel.transform.position.x, panel.transform.position.y + 120 - 5 * y * level,
                panel.transform.position.z), Quaternion.identity, mapGameObject.transform);
        var point = newCircle.GetComponent<Circle>();
        point.step = step;
        point.level = level;
        points[point.level].Add(point);
        //point.mapCircleObject = newCircle;
        //point.taskPoint = point;

        newCircle.GetComponent<Button>().onClick.AddListener(delegate { taskManager.UpdateTargetStep(step); });

        for (int i = 0; i < points[point.level].Count; i++)
        {
            points[point.level][i].transform.position = new Vector3(
                panel.transform.position.x - (points[point.level].Count - 1) * x / 2 + i * x,
                points[point.level][i].transform.position.y, points[point.level][i].transform.position.z);
        }

        //allPoints.Add(newCircle);
        //SetState(point.mapCircleObject, state);

        point.updateState();
    }
}