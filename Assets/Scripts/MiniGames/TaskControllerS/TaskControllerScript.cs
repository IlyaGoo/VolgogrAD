using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TaskControllerScript : NetworkBehaviourExtension
{
    TaskLogic thisLogic;

    public bool needFreeMobsAfterEnd = true;
    public int needBotsCount = 10;
    public bool canReturnMobs = true;//Можем ли мы отдать с этого таска ботов, например, с раскопок не можем

    public TaskMenuScript menuScript;
    public Menu currentMenu;
    /** Stages времени, в которых этот таск активен **/
    public string taskName;

    [SerializeField] public MiniGameController[] _minigame;
    //0-стол
    public List<int> currentMinigames = new List<int>();
    private bool started = false;
    public bool isOn = false;
    //public  minigamesInformation = new
    public TaskTree currentPlan => currentPlanNum == -1 ? null : plans[currentPlanNum];
    public int currentPlanNumber;
    int currentPlanNum;
    public bool needMap = false;
    public bool needShowButtons;
    public TaskType currentType;
    bool inited = false;
    private MiniGameController choosenMinigame
    {
        get
        {
            if (menuScript.targetMenu == null || menuScript.targetMenu.targetButton == null)
                return null;
            else
                return menuScript.targetMenu.targetButton.gameController;

        }
    }

    TaskTree tushnyak => new TaskTree(new PointData[] {
        new PointData("Нарезать лук и морковь", 0, 3),
        new PointData("Смешать сосус", 0, 3),
        new PointData("Засыпать гречу", 1, 3),
        new PointData("Обжарить лук", 0, 3, new int[]{0}),
        new PointData("Обжарить тушенку", 0, 3),
        new PointData("Слить воду", 1, 3, new int[]{2}),
        new PointData("Добивить смесь", 0, 3, new int[]{3, 1}),
        new PointData("Добавить и приготовить", 1, 3, new int[]{4, 5, 6})
        }, "Греча с тушонкой"
    );

    TaskTree rascop => new TaskTree(new PointData[1] {
        new PointData("Придти на костыльное плато", 0, 1)
        }, "Раскоп"
    );

    TaskTree postr => new TaskTree(new PointData[1] {
        new PointData("", 0, 1)
        }, "Раскоп"
    );

    TaskTree[] plans; //массив последовательностей действий

    public void Reinitializate(int type, bool needMenu, bool needButtons, string newName)
    {
        if (inited) throw new ArgumentException("Как ты, блять, вызвал реинициализацию таск контроллера второй раз");
        inited = true;
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
                plans = new TaskTree[1] { tushnyak };
                break;
            case TaskType.Diging:
                plans = new TaskTree[1] { rascop };
                break;
            case TaskType.Standing:
                plans = new TaskTree[1] { postr };
                break;
            case TaskType.Sleeping:
                plans = new TaskTree[1] { postr };
                break;
            default:
                throw new ArgumentOutOfRangeException("В TaslController не указаны plans для такого типа тасков");
        }

        if (isServer)
        {
            GameSystem.instance.localPlayer.GetComponent<Commands>().CmdSetPlan(gameObject, Random.Range(0, plans.Length));
            thisLogic = mobsManager.AddTask(this);
        }
    }

    public void End()
    {
        if (!inited) return; //Скорее всего возожно, если игрок подключился во время завершения
        foreach(var m in _minigame)
        {
            m.EndGame();
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        if (currentPlan != null) currentPlan.CloseMap();

        var a = GetComponent<TriggerAreaDoing>();
        if (a != null)
            a.TurnLabel(false);
        isOn = false;
        
        if (currentMenu != null)
        {
            menuScript.RemoveMenu(currentMenu);
            currentMenu.DestroyOwnObject();
            currentMenu = null;
        }

        mobsManager.RemoveTask(thisLogic);
        inited = false;
    }

    public void ChangePlan(int n)
    {
        if(!inited) return;//Скорее всего возможно, если чел подключился во время того, как пришло это сообщение
        currentPlanNum = n;
        if (needMap)
        {
            currentPlan.map = GetComponent<MapScript>();
            currentPlan.SpawnMap();
        }
    }

    public void MinigameEnded(int num, TaskPoint taskPoint) //контроллер миниигры сообщает, что ее закончили + свой номер в current_minigames
    {
        var taskWasTarget = _minigame[num].ownButton.isTarget;
        
        _minigame[num].ChangeTargetState(false);
        
        currentMenu.removeButton(_minigame[num].ownButton);
        
        currentMinigames.Remove(num);

        //if (currentType != TaskType.Coocking) return;

        var newEnablePoints = currentPlan.EndPoint(taskPoint);

        if (needMap)
        {
            var mapScript = GetComponent<MapScript>();
            mapScript.SetState(taskPoint.mapCircleObject, CircleState.Ended);
        }
        TurnOnSpaces();

        if (taskWasTarget)//Если таск был активным, нужно выбрать новый активный
        {
            if (currentMinigames.Contains(num))//Желательно на том же месте
            {
                ChangeTarget(_minigame[num].taskPoint);
            }
            else if (currentMinigames.Count > 0)//Если нет, то в первом попавшемся месте
            {
                ChangeTarget(_minigame[currentMinigames[0]].taskPoint);
            }
        }
    }

    void TurnOnSpaces()
    {
        for (int i = 0; i < currentPlan.allPoints.Count; i++)
        {
            var point = currentPlan.allPoints[i];
            var miniGameNum = point.pointData.minigameControllerNum;
            if (point.enableNow && !currentMinigames.Contains(miniGameNum))
            {
                StartGame(miniGameNum, point.needCount, point.pointData.name, point);
                currentMinigames.Add(miniGameNum);
                
            }
        }
        if (currentMinigames.Count == 0)
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

    public void StartGame(int num, int need, string name, TaskPoint taskPoint)
    {
        _minigame[num].gameObject.SetActive(true);
        _minigame[num].MinigameStart(this, num, need, name, taskPoint);
        var thisButton = needShowButtons ? currentMenu.addButton(name, _minigame[num].transform, _minigame[num]) : null;
        _minigame[num].ownButton = thisButton;
        
        if (currentMenu.miniButtons.Count == 1)//Если это первая кнопка в меню, нужно сделать все активным
        {
            if (currentMenu.isTarget)
            {
                currentMenu.ChangeTarget(false);
                ChangeTarget(taskPoint);
            }
        }
        else if(needShowButtons)//Иначе нужно накинуть на нее, что она готова, но не выбрана
        {
            _minigame[num].ChangeTargetState(false);
        }
    }

    public void ChangeTarget(TaskPoint taskPoint)
    {
        if (taskPoint.enableNow && _minigame[taskPoint.pointData.minigameControllerNum].ready)
        {
            if (choosenMinigame != null && choosenMinigame.taskPoint == taskPoint) //Если нажали на кнопку активного таска
            {
                choosenMinigame.ChangeTargetState(false);
            }
            else
            {
                if (choosenMinigame != null)
                    choosenMinigame.ChangeTargetState(false);
                _minigame[taskPoint.pointData.minigameControllerNum].ChangeData(this, 3, taskPoint);
                _minigame[taskPoint.pointData.minigameControllerNum].ChangeTargetState(true);
            }
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
