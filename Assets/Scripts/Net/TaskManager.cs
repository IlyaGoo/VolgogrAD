using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

public class TaskManager : NetworkBehaviourExtension {
    public static TaskManager instance;

    public Transform toolsPanelTransform;
    public Transform mouseInventoryPanelTransform;

    public GameObject canvas;
    public GameObject invPanelLeft;
    public GameObject invPanelRight;
    public OptionsScript optionsScript;
    public CampEnterAreaDoing[] campsAreas;
    public CarAreaDoing[] carsAreas;
    public List<GameObject> wholes = new List<GameObject>();

    private readonly List<LightScript> lightGameObjects = new List<LightScript>();
    public float currentLightLevel = 1;
    [SerializeField] GameObject[] nightObjects;

    public int gameTimer;

    bool inited = false;

    public float currentTime = 740;

    public int playerCount = 1;

    [SerializeField] float normalTimeMode = 8;
    [SerializeField] float speedTimeMode = 0.1f;
    private float currentTimeMode;

    private readonly Dictionary<string, float> playersSkipDict = new Dictionary<string, float>();
    private int endSkipTime = 0;
    
    [SerializeField] private Text timerText;
    
    private int timeMultyplayer = 1;

    public bool taskCanBechanged = true;

    public bool needToUpdate = true;

    public Light2D globalLight = null;
    public readonly List<TaskControllerScript> taskControllers = new List<TaskControllerScript>();

    /** Объекты, на которых висит TaskControllerScript, типа контроллера на кухне, построения итд**/
    [SerializeField] private GameObject[] taskControllerObjects;

    /** Стандартный шаблон на день**/
    public List<TaskInfo> standartDayTemplate;
    /** Таски, которые выполняются прямо сейчас**/
    public List<TaskInfo> currentTasks = new List<TaskInfo>();
    /** Таски, которые осталось выполнить в этот день**/
    public List<TaskInfo> remainingTasks = new List<TaskInfo>();
    
    /** Стандартный шаблон каких-то событий на день, типа затемнения светящихся объектов итд**/
    public static List<AbstractTimeDoing> standartDayTimeDoingTemplate = new List<AbstractTimeDoing>()
    {
        new SetLightObjectsTimeDoing(420, false, false),
        new SetLightObjectsTimeDoing(1230, true, false),
        new SetLightLevelOnce(0, 0, 1, false),
        new SetLightLevelOnce(420, 0.8f, 0, false),
        new SendBotsToSleepTimeDoing(220, true),
        new SetLightLevelUntilMidnightTimeDoing(1230, 1440, false),
        new SetLightLevelAfterMidnightTimeDoing(220, 420, false)
    };
    /** Действия, которые нужно еще сделать в какое-то время**/
    public List<AbstractTimeDoing> remainingTimeDoings = new List<AbstractTimeDoing>();
    /** Действия, которые нужно делать в какое-то время**/
    public List<AbstractTimeDoing> currentTimeDoings = new List<AbstractTimeDoing>();
    
    private TaskManager()
    { }
    
    private void initTemplates()
    {
        standartDayTemplate = new List<TaskInfo>(){
            new TaskInfo(taskControllerObjects[3], "Сон", TaskType.Sleeping, false, false, 0 ,420, true),
            new TaskInfo(taskControllerObjects[0], "Приготовить завтрак", TaskType.Coocking, true, true, 420, 600, false),
            new TaskInfo(taskControllerObjects[2], "Построение", TaskType.Standing, false, false, 600, 630, true),
            new TaskInfo(taskControllerObjects[1], "Раскопки", TaskType.Diging, false, false, 630, 870, false),
            new TaskInfo(taskControllerObjects[0], "Приготовить обед", TaskType.Coocking, true, true, 720, 900, false),
            new TaskInfo(taskControllerObjects[1], "Раскопки", TaskType.Diging, false, false, 990, 1170, false),
            new TaskInfo(taskControllerObjects[0], "Приготовить ужин", TaskType.Coocking, true, true, 1020, 1200, false)
        };
    }

    private void Awake()
    {
        initTemplates();
        instance = this;
    }

    public void Init()
    {
        //LocalPlayer.GetComponent<PlayerNet>().RpcSendNickName();
        SetDayDoingsTimplate();
        if (isServer)
        {
            inited = true;
            SetDayTimplate();
        }
        currentTimeMode = normalTimeMode;
    }

    public void AddLightoObject(LightScript obj)
    {
        lightGameObjects.Add(obj);
        obj.SetLight(currentLightLevel);
    }

    public void RemoveLightObject(LightScript obj)
    {
        lightGameObjects.Remove(obj);
    }

    public void Connected()
    {
        playerCount++;
    }
    
    public void SetData(int time, GameObject[] startedTasks, int[] plans)
    {
        SetGameTimer(time);

        for(var i = 0; i < startedTasks.Length; i++)
        {
            var cont = startedTasks[i].GetComponent<TaskControllerScript>();
            cont.ChangePlan(plans[i]);
            cont.StartGames();
        }

        inited = true;
    }

    void CheckEndStartTasks()
    {
        if (!isServer) return;
        var needRemoveFromCurrentTasks = new List<TaskInfo>();
        var needRemoveFromRemainingTasks = new List<TaskInfo>();
        
        foreach (var task in currentTasks.Where(task => gameTimer >= task.endTime))//Сначала заканчиваем таски, вдруг новый таск будет на том же месте
        {
            localCommands.RpcEndTaskController(task.currentScript.gameObject);
            taskControllers.Remove(task.currentScript);
            needRemoveFromCurrentTasks.Add(task);
        }
        
        foreach (var task in needRemoveFromCurrentTasks)
            currentTasks.Remove(task);
        
        foreach (var task in remainingTasks.Where(task => gameTimer >= task.startTime))
        {
            task.currentScript = CreateTask(task.taskObject, task.newTaskName, task.type, task.needButtons, task.needShowMenu);
            currentTasks.Add(task);
            if (task.needStartAfterCreate)
                localCommands.CmdStartGames(task.taskObject);
            
            needRemoveFromRemainingTasks.Add(task);
        }
        
        TaskControllerScript CreateTask(GameObject taskObject, string newTaskName, TaskType type, bool needButtons, bool needShowMenu)
        {
            var taskController = taskObject.GetComponent<TaskControllerScript>();//cooker, digger итд
            localCommands.CmdInitTaskController(taskObject, (int)type, needShowMenu, needButtons, newTaskName);
            taskControllers.Add(taskController);
            return taskController;
        }
        
        foreach (var task in needRemoveFromRemainingTasks)
            remainingTasks.Remove(task);
    }
    
    void CheckEndStartTimeDoings()
    {
        var needRemoveFromCurrentTimeDoings = new List<AbstractTimeDoing>();
        var needRemoveFromRemainingTimeDoings = new List<AbstractTimeDoing>();
        
        foreach (var task in remainingTimeDoings.Where(task => gameTimer >= task.startTime))
        {
            currentTimeDoings.Add(task);
            needRemoveFromRemainingTimeDoings.Add(task);
        }

        foreach (var task in currentTimeDoings.Where(task => gameTimer >= task.endTime))
        {
            needRemoveFromCurrentTimeDoings.Add(task);
        }

        foreach (var doing in currentTimeDoings)
            doing.Do();

        foreach (var task in needRemoveFromCurrentTimeDoings)
            currentTimeDoings.Remove(task);
        foreach (var task in needRemoveFromRemainingTimeDoings)
            remainingTimeDoings.Remove(task);
    }
    

    void SetGameTimer(int newGameTimer)
    {
        gameTimer = newGameTimer;
        if (!GameSystem.instance.allInited) return;//Не должны ничего делать, пока не инициализируем все полностью
        if (gameTimer > 1439)
        {
            if (endSkipTime != 0)
            {
                endSkipTime -= 1439;
                if (endSkipTime == 0)
                    CleanSkip();
            }
            gameTimer = 0;
            currentTime = 0;
            SetNewDay();
            SetDayTimplate();//Заполняем таски на день
            SetDayDoingsTimplate();//Заполняем действия типа затемнения
        }
        var val1 = gameTimer / 60;
        var val2 = gameTimer % 60;
        var hourText = (val1 < 10 ? ("0" + val1) : (val1).ToString());
        var minutsText = (val2 < 10 ? ("0" + val2) : (val2).ToString());
        UpdateTimer(hourText + ":" + minutsText);
        CheckEndStartTimeDoings();

        if (taskCanBechanged) CheckEndStartTasks();

        if (endSkipTime != 0 && gameTimer > endSkipTime) CleanSkip();
    }

    public void SetLight(float intensy, float secondIntensy)
    {
        SetConnectionLight(Mathf.Max(0, intensy + 0.2f), Mathf.Max(0, secondIntensy - globalLight.intensity));
    }

    public void SetConnectionLight(float intensy, float secondIntensy)
    {
        globalLight.intensity = intensy;
        currentLightLevel = secondIntensy;

        foreach (var l in lightGameObjects)
            l.SetLight(currentLightLevel);

    }

    public void SetNightObject(bool state)
    {
        foreach (var e in nightObjects)
            e.SetActive(state);
    }

    private void CleanSkip()
    {
        playersSkipDict.Clear();
        StopSkip();
        RpcVakeUp();
    }

    [ClientRpc]
    void RpcVakeUp()
    {
        if (localTaker.currentAreaDoing != null)
        {
            var scr = localTaker.currentAreaDoing.GetComponent<SleepAreaDoing>();
            if (scr != null)
                scr.WakeUp();
        }
    }

    void UpdateTimer(string newText)
    {
        timerText.text = (newText);
    }

    private string getName(Task needTask)
    {
        switch (needTask)
        {
            case Task.Digging:
                return ("Раскопки");
            case Task.Eating:
                return ("Еда");
            case Task.Сooking:
                return ("Готовка");
            case Task.Sleep:
                return ("Сон");
            case Task.Parade:
                return ("Построение");
            case Task.Free:
                return ("Нету");
            default:
                return ("");
        }
    }

    [ClientRpc] 
    void RpcSendTime(int time)
    {
        SetGameTimer(time);
    }

    public SleepAreaDoing nowSleepObject = null;

    void SetNewDay()
    {
        if (nowSleepObject != null)
        {
            nowSleepObject.SendReadyToSkip(localPlayerId);
        }
    }

    void SetDayTimplate()
    {
        remainingTasks = new List<TaskInfo>(standartDayTemplate); //Строим текущий план на день
        currentTasks.Clear();
    }
    
    void SetDayDoingsTimplate()
    {
        remainingTimeDoings = new List<AbstractTimeDoing>(standartDayTimeDoingTemplate); //Строим текущий план действий на день
        currentTimeDoings.Clear();
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (isServer)
        {
            currentTime += Time.fixedDeltaTime * timeMultyplayer;
            var prGameTimer = gameTimer;
            gameTimer = (int)(currentTime/ currentTimeMode);
            if (prGameTimer != gameTimer)
                RpcSendTime(gameTimer);
        }
	}

    public enum Task
    {
        Eating,
        Сooking,
        Digging,
        Sleep,
        Parade,
        Free
    }

    public void ReadyToSkip(string id, int newTime)
    {
        //endSkipTime = (int)newTime;
        if (playersSkipDict.ContainsKey(id))
            playersSkipDict[id] = newTime;
        else
            playersSkipDict.Add(id, newTime);
        Sleep(false);
    }

    public void UnReadyToSkip(string id, bool removing)
    {
        if (playersSkipDict.ContainsKey(id))
            playersSkipDict.Remove(id);
        Sleep(removing);
    }

    private void Sleep(bool removing)
    {
        if (playersSkipDict.Count != EntitysController.instance.playersData.Count - (removing ? 1 : 0)) 
        {
            StopSkip();
            return;
        }
        SkipTo((int)playersSkipDict.Values.Min());
        if (isServer)
            foreach (var mini in GameObject.FindGameObjectsWithTag("SleepMiniGame"))
            {
                mini.GetComponent<SleepMiniGame>().AllSleep();
            }
    }

    public void SkipTo(int endTime, int mult = 320)
    {
        //if (endTime > 1439) return;
        localCommands.RpcSetEnergyMultiplayer(100);
        endSkipTime = endTime;
        timeMultyplayer = mult;
        GetComponent<ObjectsScript>().botSpeedMultiplayer = 4;
    }

    void StopSkip()
    {
        timeMultyplayer = 1;
        localCommands.RpcSetEnergyMultiplayer(1);
        endSkipTime = 0;
        GetComponent<ObjectsScript>().botSpeedMultiplayer = 1;
    }
}