using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Объект в лагере, на котором может происходить миниигра по заданию
 * Например: костер на которой может быть готовка
 */
public class CampObject : TriggerAreaDoing
{
    public static readonly List<CampObject> AllCampObjects = new();
    public CampObjectType objectType;

    public Point ownPoint;

    public static List<CampObject> getByType(CampObjectType objectType)
    {
        return AllCampObjects.FindAll(obj => obj.objectType == objectType);
    }

    private void Awake()
    {
        AllCampObjects.Add(this);
    }

    public override bool NeedShowLabel()
    {
        return QuestStepsController.Instance.GetByCampObjectType(objectType) != null && PlayerThere;
    }

    public override bool CanInteract(GameObject interactEntity)
    {
        return QuestStepsController.Instance.GetByCampObjectType(objectType) != null;
    }

    public override void UpdateText()
    {
        var step = QuestStepsController.Instance.GetByCampObjectType(objectType);
        if (step != null) ChangeText(step.stepName);
    }

    public override bool Do()
    {
        var step = QuestStepsController.Instance.GetByCampObjectType(objectType);
        if (step == null) return false;
        step.SpawnMiniGame();
        return true;
    }
}

public enum CampObjectType
{
    Nothing = -1,
    Campfire = 0,
    Kitchen = 1,
    MainPoint = 2, //Точка сбора
}