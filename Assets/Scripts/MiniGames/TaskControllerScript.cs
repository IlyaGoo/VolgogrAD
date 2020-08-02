using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TaskControllerScript : NetworkBehaviour
{
    public MobsManager _mobManager;
    TaskLogic thisLogic;
    public TaskMenuScript menuScript;
    public Menu currentMenu;
    public int[] inStages;
    public string taskName;

    [SerializeField] public MiniGameController[] _minigame;
    //0-стол
    public List<int> current_minigames = new List<int>();
    private bool started = false;
    public bool isOn = false;
    //public  minigamesInformation = new
    TaskData CurrentPlan { get {
            if (currentPlanNum == -1) return null;
            return plans[currentPlanNum]; } }
    public int currentPlanNumber;
    int currentPlanNum;
    public bool needMap = false;
    public bool needShowButtons;
    public TaskType currentType;
    bool inited = false;

    static readonly TaskData tushnyak = new TaskData(new PointData[] {
        new PointData(new MiniGameInfo("Нарезать лук и морковь", 0, 3)),
        new PointData(new MiniGameInfo("Смешать сосус", 0, 3)),
        new PointData(new MiniGameInfo("Засыпать гречу", 1, 3)),
        new PointData(new MiniGameInfo("Обжарить лук", 0, 3), new int[]{0}),
        new PointData(new MiniGameInfo("Обжарить тушенку", 0, 3)),
        new PointData(new MiniGameInfo("Слить воду", 1, 3), new int[]{2}),
        new PointData(new MiniGameInfo("Добивить смесь", 0, 3), new int[]{3, 1}),
        new PointData(new MiniGameInfo("Добавить и приготовить", 1, 3), new int[]{4, 5, 6})
        }, "Греча с тушонкой"
    );

    static readonly TaskData rascop = new TaskData(new PointData[1] {
        new PointData(new MiniGameInfo("Придти на костыльное плато", 0, 1))
        }, "Раскоп"
    );

    static readonly TaskData postr = new TaskData(new PointData[1] {
        new PointData(new MiniGameInfo("", 0, 1))
        }, "Раскоп"
    );

    TaskData[] plans; //массив рецептов

    public void Reinitializate(int type, bool needMenu, bool needButtons, string newName, int[] stages)
    {
        if (inited) return;
        inited = true;
        inStages = stages;
        taskName = newName;
        currentMenu = menuScript.AddMenu(newName, transform, needMenu, GetComponent<MapScript>());
        needShowButtons = needButtons;
        needMap = needMenu;
        currentType = (TaskType)type;
        isOn = true;
        started = false;
        GetComponent<BoxCollider2D>().enabled = true;

        switch (currentType) //Нужно присвоить текущее граф таска, типа что за чем следует
        {
            case TaskType.Coocking:
                plans = new TaskData[1] { tushnyak };
                break;
            case TaskType.Diging:
                plans = new TaskData[1] { rascop };
                break;
            case TaskType.Standing:
                plans = new TaskData[1] { postr };
                break;
            case TaskType.Sleeping:
                plans = new TaskData[1] { postr };
                break;
        }

        /*if (needMap)*/
        /*    foreach (var plan in plans)*/
        /*        plan.map = GetComponent<MapScript>();*/

        if (isServer)
        {
            GameSystem.instance.localPlayer.GetComponent<Commands>().CmdSetPlan(gameObject, Random.Range(0, plans.Length));
            thisLogic = _mobManager.AddTask(this);
        }
    }

    public void End()
    {
        foreach(var m in _minigame)
        {
            m.EndGame();
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        if (CurrentPlan != null) CurrentPlan.CloseMap();

        var a = GetComponent<TriggerAreaDoing>();
        if (a != null)
            a.TurnLabel(false);
        isOn = false;



        if (currentMenu == null) return;
        menuScript.RemoveMenu(currentMenu);
        currentMenu.DestroyOwnObject();
        currentMenu = null;
        _mobManager.RemoveTask(thisLogic);
        inited = false;
    }

    public void ChangePlan(int n)
    {
        currentPlanNum = n;
        if (needMap)
        {
            CurrentPlan.map = GetComponent<MapScript>();
            CurrentPlan.SpawnMap();
        }
    }

    public void MinigameEnded(int num, int pointNum) //контроллер миниигры сообщает, что ее закончили + свой номер в current_minigames
    {
        currentMenu.removeButton(_minigame[num].ownButton);
        current_minigames.Remove(num);

        //if (currentType != TaskType.Coocking) return;

        var newEnablePoints = CurrentPlan.EndPoint(pointNum);

        if (needMap)
        {
            var mapScript = GetComponent<MapScript>();
            mapScript.SetState(pointNum, CircleState.Ended);
            foreach (var p in newEnablePoints)
            {
                mapScript.SetState(p.number, CircleState.Enable);
            }
        }
        TurnOnSpaces();
    }

    void TurnOnSpaces()
    {
        for (int i = 0; i < CurrentPlan.allPoints.Count; i++)
        {
            var point = CurrentPlan.allPoints[i];
            var miniGameNum = point.minigame.minigameControllerNum;
            if (point.enableNow && !current_minigames.Contains(miniGameNum))
            {
                StartGame(miniGameNum, point.needCount, point.minigame.name, i, point.number);
                current_minigames.Add(miniGameNum);
            }
        }
        if (current_minigames.Count == 0)
        {
            End();
        }
    }

    public void StartGames()
    {
        if (started) return;

        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        started = true;

        TurnOnSpaces();
    }

    public void StartGame(int num, int need, string name, int pointNum, int number)
    {
        _minigame[num].gameObject.SetActive(true);
        var thisButton = needShowButtons ? currentMenu.addButton(name, _minigame[num].transform, pointNum) : null;
        _minigame[num].MinigameStart(this, num, need, name, pointNum, thisButton, number);

    }

    public void ChangeMinigame(int pointNum)
    {
        if (!CurrentPlan.allPoints[pointNum].enableNow || !_minigame[CurrentPlan.allPoints[pointNum].minigame.minigameControllerNum].ready) return;
        _minigame[CurrentPlan.allPoints[pointNum].minigame.minigameControllerNum].ChangeData(this, CurrentPlan.allPoints[pointNum].minigame.minigameControllerNum, 3, CurrentPlan.allPoints[pointNum].minigame.name, pointNum, CurrentPlan.allPoints[pointNum].number);

    }

    class MiniGameInfo
    {
        public string name;
        public int minigameControllerNum;
        public int needCount;

        public MiniGameInfo(string startName, int strartMinigameControllerNum, int need) {
            needCount = need;
            name = startName;
            minigameControllerNum = strartMinigameControllerNum;
        }
    }

    class TaskData
    {
        public string name;
        public MapScript map;
        public List<TaskPoint> allPoints = new List<TaskPoint>(); //Все этапы готовки
        //TaskPoint[] startPoint; //точки, которые доступные сразу

        public TaskData(PointData[] points, string newName)
        {
            name = newName;
            foreach (var point in points)
            {
                AddPoint(point.neeedPointsNums, point.miniGameInfo);
            }
        }

        public void CloseMap()
        {
            if (map != null)
                map.CloseMap();
        }

        public void SpawnMap()
        {
            map.SpawnStartObjects();
            map.ChangeMainText(name);
            for (var i = 0; i < allPoints.Count; i++)
            {
                var state = allPoints[i].enableNow ? CircleState.Enable : allPoints[i].ended ? CircleState.Ended : CircleState.Disable;
                map.AddCircle(allPoints[i].minigame.name, allPoints[i].level, allPoints[i].needPointsNums, state);
            }
        }

        public void AddPoint(int[] needPointsNums, MiniGameInfo gameInfo)
        {
            TaskPoint newPoint = new TaskPoint(gameInfo)
            {
                needCount = gameInfo.needCount,
                number = allPoints.Count
            }; //Создаем новую точку
            allPoints.Add(newPoint);
            int toLevel = 0;
            if (needPointsNums == null) { newPoint.enableNow = true; } //Если нет нужных точек, делаем сразу доступной
            else
            {
                newPoint.enableNow = false; //Если есть, то недоступной
                newPoint.needPoints = new List<TaskPoint>(); //Создаем список нужных точек
                newPoint.needPointsNums = needPointsNums;
                foreach (var p in needPointsNums)
                {
                    toLevel = Mathf.Max(allPoints[p].level + 1, toLevel);
                    newPoint.needPoints.Add(allPoints[p]); //Добавляем нужные точки из списка их номеров
                    allPoints[p].toPoint.Add(newPoint); //Добавляем нужным точкам ссылку на эту
                }
                newPoint.level = toLevel;
            }
        }

        public List<TaskPoint> EndPoint(int num)
        {
            allPoints[num].ended = true;
            allPoints[num].enableNow = false;
            var result = new List<TaskPoint>();
            //Сделать allPoint[num] не активным, убрать из отображения
            foreach (var point in allPoints[num].toPoint)
            {
                int i = 0;
                foreach(var ownPoint in point.needPoints)
                {
                    if (ownPoint.ended == false) break;
                    i++;
                }
                if (i == point.needPoints.Count) { result.Add(point); point.enableNow = true; }
                    //Сделать это точку активной, добавить отображение
            }
            return result;
        }

        public class TaskPoint //по сути единица готовки (обжарить лук, добавить пасту, помешать)
        {
            public MiniGameInfo minigame;
            public int needCount;
            //public MiniGameInfo minigame;
            //public TaskData currentTaskData;
            public ButtonInfo button = null;
            public bool enableNow;
            public int level;
            public bool ended = false;
            public List<TaskPoint> needPoints;
            public int[] needPointsNums;
            public List<TaskPoint> toPoint = new List<TaskPoint>();
            public int number;

            public TaskPoint(MiniGameInfo info)
            {
                minigame = info;
            }
        }
    }

    class PointData
    {
        public int[] neeedPointsNums;
        public MiniGameInfo miniGameInfo;

        public PointData(MiniGameInfo newMiniGameInfo, int[] newNeedPointsNums = null)
        {
            neeedPointsNums = newNeedPointsNums;
            miniGameInfo = newMiniGameInfo;
        }
    }
}

public enum TaskType
{
    Coocking = 0, 
    Diging = 1,
    Standing = 2,
    Sleeping = 3
}
