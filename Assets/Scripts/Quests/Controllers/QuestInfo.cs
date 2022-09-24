using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestInfo
    {
        public string name;
        public List<QuestStepInfo> allSteps = new List<QuestStepInfo>();

        public QuestInfo(QuestStepInfo[] steps, string newName)
        {
            name = newName;
            foreach (var step in steps)
            {
            //     AddPoint(step.neeedPointsNums, step);
                allSteps.Add(step);
            }
        }
        

        public void SpawnMap()
        {
            // map.SpawnStartObjects();
            // map.ChangeMainText(name);
            // foreach (var point in allSteps)
            // {
            //     var state = point.enableNow ? StepState.Enable : point.ended ? StepState.Ended : StepState.Disable;
            //     map.AddCircle(point, state);
            // }
        }

        // public void AddPoint(int[] needPointsNums, PointData gameInfo)
        // {
        //     // QuestStepInfo newPoint = new QuestStepInfo(gameInfo)
        //     // {
        //     //     number = allSteps.Count
        //     // }; //Создаем новую точку
        //     // allSteps.Add(newPoint);
        //     // int toLevel = 0;
        //     // if (needPointsNums == null) { newPoint.enableNow = true; } //Если нет нужных точек, делаем сразу доступной
        //     // else
        //     // {
        //     //     newPoint.enableNow = false; //Если есть, то недоступной
        //     //     newPoint.fromPoints = new List<QuestStepInfo>(); //Создаем список нужных точек
        //     //     newPoint.needPointsNums = needPointsNums;
        //     //     foreach (var p in needPointsNums)
        //     //     {
        //     //         toLevel = Mathf.Max(allSteps[p].level + 1, toLevel);
        //     //         newPoint.fromPoints.Add(allSteps[p]); //Добавляем нужные точки из списка их номеров
        //     //         allSteps[p].toPoint.Add(newPoint); //Добавляем нужным точкам ссылку на эту
        //     //     }
        //     //     newPoint.level = toLevel;
        //     // }
        // }

        // public List<QuestStepInfo> EndPoint(QuestStepInfo questStepInfo)
        // {
        //     questStepInfo.ended = true;
        //     questStepInfo.enableNow = false;
        //     var result = new List<QuestStepInfo>();
        //     //Сделать allPoint[num] не активным, убрать из отображения
        //     foreach (var point in questStepInfo.toPoint)
        //     {
        //         int i = 0;
        //         foreach(var ownPoint in point.fromPoints)
        //         {
        //             if (ownPoint.ended == false) break;
        //             i++;
        //         }

        //         if (i == point.fromPoints.Count)
        //         {
        //             result.Add(point);
        //             point.enableNow = true;
        //             map.SetState(point.mapCircleObject, StepState.Enable);
        //         } //Сделать это точку активной, добавить отображение
        //     }
        //     return result;
        // }
    }
    
public class QuestStepInfo //по сути единица готовки (обжарить лук, добавить пасту, помешать)
{
    public bool needChangeCollider;
    public int needCount;
    public GameObject questObject;
    public bool miniGameNeedEnergy;
    public string stepName;
    public bool needActive;
    public string miniGameNamePrefab; //todo
    public Vector2 size;
    public Vector2 offset;
    public int needObjectType;
    public List<int> previousStepsNumbers;

    public QuestStepInfo(
        bool newNeedChangeCollider,
        int newNeedCount,
        bool newMiniGameNeedEnergy,
        string newName,
        bool newNeedActive,
        string newMiniGameNamePrefab, //todo
        int newNeedObjectType,
        List<int> newPreviousStepsNumbersNumbers
    )
    {
        needChangeCollider = newNeedChangeCollider;
        needCount = newNeedCount;
        miniGameNeedEnergy = newMiniGameNeedEnergy;
        stepName = newName;
        needActive = newNeedActive;
        miniGameNamePrefab = newMiniGameNamePrefab;
        needObjectType = newNeedObjectType;
        previousStepsNumbers = newPreviousStepsNumbersNumbers;
    }
}

public enum StepState
{
    Chosen, 
    Enable, 
    Disable, 
    Ended
}