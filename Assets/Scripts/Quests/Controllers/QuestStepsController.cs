using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/** Скрипт, осуществляющий допонительную координацию шагов квеста */
public class QuestStepsController : MonoBehaviour
{
    public static QuestStepsController instance;

    Dictionary<CampObjectType, List<QuestStep>> campObjectToSteps = new Dictionary<CampObjectType, List<QuestStep>>();

    // Start is called before the first frame update
    void Awake()
    {
        var allCampTypes = Enum.GetValues(typeof(CampObjectType));
        foreach (CampObjectType campType in allCampTypes)
        {
            campObjectToSteps.Add(campType, new List<QuestStep>());
        }
        instance = this;
    }

    public QuestStep GetByCampObjectType(CampObjectType campType)
    {
        var steps = campObjectToSteps[campType];
        if (steps.Count == 0)
            return null;
        else
            return steps[0];
    }

    public void ResortSteps(CampObjectType sortCampType)
    {
        var cleanedList = campObjectToSteps[sortCampType].FindAll(e => e != null);
        campObjectToSteps[sortCampType] = cleanedList.OrderBy(x => x.state).ToList<QuestStep>();
    }

    public void AddStep(CampObjectType needCampType, QuestStep step)
    {
        campObjectToSteps[needCampType].Add(step);
        ResortSteps(needCampType);
    }

    public void RemoveStep(CampObjectType needCampType, QuestStep step)
    {
        campObjectToSteps[needCampType].Remove(step);
        ResortSteps(needCampType);
    }
}
