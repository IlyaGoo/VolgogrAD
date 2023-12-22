using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/** Скрипт, осуществляющий допонительную координацию шагов квеста */
public class QuestStepsController : MonoBehaviourExtension
{
    public static QuestStepsController Instance;

    private readonly Dictionary<CampObjectType, List<QuestStep>> _campObjectToSteps = new();

    private void Awake()
    {
        var allCampTypes = Enum.GetValues(typeof(CampObjectType));
        foreach (CampObjectType campType in allCampTypes)
        {
            _campObjectToSteps.Add(campType, new List<QuestStep>());
        }
        Instance = this;
    }

    public QuestStep GetByCampObjectType(CampObjectType campType)
    {
        var steps = _campObjectToSteps[campType].FindAll(step => step.state is QuestState.Available);
        //В первую очередь хотим взять таргетный шаг
        var targetStep = steps.Find(step => step == taskManager._targetStep);
        return steps.Count == 0 ? null : targetStep == null ? steps[0] : targetStep;
    }

    private void ResortSteps(CampObjectType sortCampType)
    {
        var cleanedList = _campObjectToSteps[sortCampType].FindAll(e => e != null);
        _campObjectToSteps[sortCampType] = cleanedList.OrderBy(x => x.state).ToList();
        localTaker.UpdateCampTriggerArea(sortCampType);
    }

    public void AddStep(CampObjectType needCampType, QuestStep step)
    {
        _campObjectToSteps[needCampType].Add(step);
        ResortSteps(step.needObjectType);
    }

    public void RemoveStep(QuestStep step)
    {
        _campObjectToSteps[step.needObjectType].Remove(step);
        ResortSteps(step.needObjectType);
    }
}
