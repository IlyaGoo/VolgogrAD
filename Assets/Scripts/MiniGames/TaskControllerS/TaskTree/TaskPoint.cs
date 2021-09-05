using System.Collections.Generic;
using UnityEngine;

public class TaskPoint //по сути единица готовки (обжарить лук, добавить пасту, помешать)
{
    public PointData pointData;
    public int needCount => pointData.needCount;

    public int currentCount = 0;
    //public MiniGameInfo minigame;
    //public TaskData currentTaskData;
    public ButtonInfo button = null;
    public GameObject mapCircleObject;
    public bool enableNow;
    public int level;
    public bool ended = false;
    public List<TaskPoint> fromPoints;
    public int[] needPointsNums;
    public List<TaskPoint> toPoint = new List<TaskPoint>();
    public int number;

    public TaskPoint(PointData data)
    {
        pointData = data;
    }
}