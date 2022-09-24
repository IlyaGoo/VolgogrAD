using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Объект в лагере, на котором может происходить миниигра по заданию
 * Например: костер на которой может быть готовка
 */
public class CampObject : TriggerAreaDoing
{

    public static readonly List<CampObject> allCampObjects = new List<CampObject>();
    public CampObjectType objectType;

    public Point ownPoint;
    
    private void Awake()
    {
        allCampObjects.Add(this);
    }

    public override bool NeedShowLabel()
    {
        return QuestStepsController.instance.GetByCampObjectType(objectType) != null && PlayerThere;
    }

    public override bool CanInteract(GameObject interactEntity)
    {
        return QuestStepsController.instance.GetByCampObjectType(objectType) != null;
    }

    public override bool Do()
    {
        var step = QuestStepsController.instance.GetByCampObjectType(objectType);
        if(step == null) return false;
            step.SpawnMiniGame(gameObject);
        return true;
    }
}

public enum CampObjectType
{
    Nothing = -1,
    Campfire = 0, 
    Kitchen = 1,
    MainPoint = 2 //Точка сбора
}
