using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

public class TaskManager : NetworkBehaviour {
    public GameObject canvas;
    public GameObject invPanelLeft;
    public GameObject invPanelRight;
    public OptionsScript optionsScript;
    public ChatHelper chatHelper;
    public CallMenu menuSctipt;
    public CampEnterAreaDoing[] campsAreas;
    public CarAreaDoing[] carsAreas;
    public RadioScript radioScript;
    public GameObject LocalPlayer;
    public List<GameObject> wholes = new List<GameObject>();

    public ObjectsScript objectsScript;
    private readonly List<LightScript> lightGameObjects = new List<LightScript>();
    public float currentLightLevel = 1;
    [SerializeField] GameObject[] nightObjects;

    public int gameTimer;
    private string timerTexts;
    public MobsManager _mobManager;

    bool inited = false;

    public float currentTime = 740;

    public int playerCount = 1;

    [SerializeField] float normalTimeMode = 8;
    [SerializeField] float speedTimeMode = 0.1f;
    private float currentTimeMode;

    private Dictionary<string, float> playersSkipDict = new Dictionary<string, float>();
    private int endSkipTime = 0;

    [SerializeField] private GameObject timerObject = null;
    public Taker _taker;
    private Text timerText;

    [SerializeField]private Text[] tasksText;
    private int timeMultyplayer = 1;

    private int currentStage = 0;

    public bool taskCanBechanged = true;

    public bool needToUpdate = true;

    public Light2D globalLight = null;

    List<TaskControllerScript> TaskControllers = new List<TaskControllerScript>();

    [SerializeField] private GameObject[] taskController;

    public Dictionary<string, PlayerInfo> PlayersDatas = new Dictionary<string, PlayerInfo>();//netId Data

    Commands _cmd;

    public Commands Cmd
    {
        get
        {
            if (_cmd == null)
            {
                _cmd = LocalPlayer.GetComponent<Commands>();
            }
            return _cmd;
        }
    }


    public PlayerInfo GetPlayerData(string id)
    {
        if (id == null)
            return null;
        if (PlayersDatas.Keys.Contains(id))
            return PlayersDatas[id];
        else
            return null;
    }

    public void Init(GameObject player)
    {
        LocalPlayer = player;
        //LocalPlayer.GetComponent<PlayerNet>().RpcSendNickName();
        if (isServer)
        {
            inited = true;
        }
        else
            Cmd.CmdConnected();
        timerText = timerObject.GetComponent<Text>();
        //SetGameTimer();
        currentTimeMode = normalTimeMode;
    }

    public void PlayerSendedData(string identity, string name, GameObject playerObject)
    {
        if (PlayersDatas.ContainsKey(identity) || !isServer) return;
        PlayersDatas.Add(identity, new PlayerInfo(name, playerObject));
        LocalPlayer.GetComponent<ChatPlayerHelper>().RpcSendLine(name + " присоединился к игре");
    }

    public void PlayerDisConnectedNickname(string identity)
    {
        if (!PlayersDatas.ContainsKey(identity)) return;
        LocalPlayer.GetComponent<ChatPlayerHelper>().RpcSendLine(PlayersDatas[identity].nickname + " вышел из игры");
        PlayersDatas.Remove(identity);
        
    }

    public void AddLightoObject(LightScript obj)
    {
        lightGameObjects.Add(obj);
        obj.SetLight(currentLightLevel);
    }

    public void RemoveLightoObject(LightScript obj)
    {
        lightGameObjects.Remove(obj);
    }

    public void Connected()
    {
        if (!isServer) return;
        playerCount++;
        GameObject[] res = new GameObject[TaskControllers.Count];
        var res2 = new int[TaskControllers.Count];
        for (var i = 0; i < TaskControllers.Count; i++)
        {
            var cont = TaskControllers[i];
            res[i] = cont.gameObject;
            res2[i] = cont.currentPlanNumber;
            Cmd.CmdInitTaskController(cont.gameObject, (int)cont.currentType, cont.needMap, cont.needShowButtons, cont.taskName, cont.inStages);
        }

        RpcSetData(currentTime, res, res2);
        //LocalPlayer.GetComponent<Commands>().CmdStartGames(task.taskController.gameObject);
    }

    [ClientRpc]
    void RpcSetData(float time, GameObject[] startedTasks, int[] plans)
    {
        if (inited) return;
        currentTime = time;
        SetGameTimer();

        for(var i = 0; i < startedTasks.Length; i++)
        {
            var cont = startedTasks[i].GetComponent<TaskControllerScript>();
            cont.ChangePlan(plans[i]);
            cont.StartGames();
        }

        inited = true;
    }

    void SetGameTimer()
    {
        gameTimer = (int)(currentTime/ currentTimeMode);
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
        }
        var val1 = gameTimer / 60;
        var val2 = gameTimer % 60;
        var hourText = (val1 < 10 ? ("0" + val1) : (val1).ToString());
        var minutsText = (val2 < 10 ? ("0" + val2) : (val2).ToString());
        timerTexts = hourText + ":" + minutsText;
        UpdateTimer();

        if (!taskCanBechanged) return;
        if (420 < gameTimer && gameTimer < 601)
        {
            if (currentStage != 1)
            {
                SetLight(0.8f, 0);
                SetNightObject(false);
                currentStage = 1;
                CheckEndTasks();
                CreateTask(taskController[0], "Приготовить завтрак", new int[] { currentStage }, TaskType.Coocking, true, true);
            }
        }
        else if (600 < gameTimer && gameTimer < 631)
        {
            if (currentStage != 2)
            {
                currentStage = 2;
                CheckEndTasks();
                CreateTask(taskController[2], "Построение", new int[] { currentStage }, TaskType.Standing, false, false);
                if (isServer) Cmd.CmdStartGames(taskController[2]);
            }
        }
        else if (610 < gameTimer && gameTimer < 631)
        {
            if (currentStage != 3)
            {
                currentStage = 3;
                //Свободное время
                CheckEndTasks();
            }
        }
        else if (630 < gameTimer && gameTimer < 721)
        {
            if (currentStage != 4)
            {
                currentStage = 4;
                CheckEndTasks();
                CreateTask(taskController[1], "Раскопки", new int[] { currentStage, currentStage+1 }, TaskType.Diging, false, false);
            }
        }
        else if (720 < gameTimer && gameTimer < 871)
        {
            if (currentStage != 5)
            {
                currentStage = 5;
                CreateTask(taskController[0], "Приготовить обед", new int[] { currentStage + 1 }, TaskType.Coocking, true, true);
            }
        }
        else if (870 < gameTimer && gameTimer < 901)
        { 
            if (currentStage != 6)
            {
                currentStage = 6;
                CheckEndTasks();
            }
        }
        else if (900 < gameTimer && gameTimer < 931)
        { //task1 = "Еда"; task2 = "";

            if (currentStage != 7)
            {
                currentStage = 7;
                CheckEndTasks();
            }
        }
        else if (930 < gameTimer && gameTimer < 991)
        { //task1 = "Свободное время"; task2 = "";
            if (currentStage != 8)
            {
                currentStage = 8;
                CheckEndTasks();
            }
        }
        else if (990 < gameTimer && gameTimer < 1021)
        { //task1 = "Раскопки"; task2 = "";
            if (currentStage != 9)
            {
                currentStage = 9;
                CheckEndTasks();
                CreateTask(taskController[1], "Раскопки", new int[] { currentStage, currentStage + 1 }, TaskType.Diging, false, false);
            }

        }
        else if (1020 < gameTimer && gameTimer < 1171)
        { //task1 = "Раскопки"; task2 = "Готовка";
            if (currentStage != 10)
            {
                currentStage = 10;
                CheckEndTasks();
                CreateTask(taskController[0], "Приготовить ужин", new int[] { currentStage, currentStage + 1 }, TaskType.Coocking, true, true);
            }
        }
        else if (1170 < gameTimer && gameTimer < 1201)
        { //task1 = "Готовка"; task2 = "";
            if (currentStage != 11)
            {
                currentStage = 11;
                CheckEndTasks();
            }

        }
        else if (1200 < gameTimer && gameTimer < 1231)
        { //task1 = "Еда"; task2 = "";
            if (currentStage != 12)
            {
                currentStage = 12;
                CheckEndTasks();
            }
        }
        else if (1230 < gameTimer && gameTimer < 1441)
        {
            SetLight(0.8f - (gameTimer - 1230) / 211f * 0.8f, (gameTimer - 1230) / 211f);
            if (currentStage != 13)
            {
                SetNightObject(true);
                //task1 = "Свободное время"; task2 = "";
                currentStage = 13;
                CheckEndTasks();
            }
        }
        else if (0 <= gameTimer && gameTimer < 421)
        { 
            if (currentStage != 14)
            {
                //task1 = "Сон"; task2 = "";
                SetLight(0 , 1);
                currentStage = 14;
                CheckEndTasks();
                CreateTask(taskController[3], "Сон", new int[] { currentStage }, TaskType.Sleeping, false, false);
                if (isServer) Cmd.CmdStartGames(taskController[3]);
            }
            else if (gameTimer > 220)
                SetLight((gameTimer - 220) / 200f * 0.8f , 1 - (gameTimer - 220) / 200f);
        }

        if (endSkipTime != 0 && gameTimer > endSkipTime) CleanSkip();
    }

    void ChangeStage(int newStage, TaskControllerScript[] newTasks)
    {
            currentStage = newStage;
            CheckEndTasks();
            foreach (var task in newTasks)
                TaskControllers.Add(task);
    }

    void CheckEndTasks()
    {
        List<TaskControllerScript> needToRemove = new List<TaskControllerScript>();
        foreach (var task in TaskControllers)
        {
            if (!task.inStages.Contains(currentStage))
            {
                task.End();
                needToRemove.Add(task);
            }
        }
        foreach (var n in needToRemove) TaskControllers.Remove(n);
    }

    public void SetLight(float intensy, float secondIntensy)
    {
        SetConnectionLight(intensy + 0.2f, secondIntensy - globalLight.intensity);
    }

    public void SetConnectionLight(float intensy, float secondIntensy)
    {
        globalLight.intensity = intensy;
        currentLightLevel = secondIntensy;

        foreach (var l in lightGameObjects)
            l.SetLight(currentLightLevel);

    }

    void SetNightObject(bool state)
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
        if (_taker.currentAreaDoing != null)
        {
            var scr = _taker.currentAreaDoing.GetComponent<SleepAreaDoing>();
            if (scr != null)
                scr.VakeUp();
        }
    }

    void UpdateTimer()
    {
        if (timerText == null) return;
        timerText.text = (timerTexts);
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
    void RpcSendTime(float time)
    {
        currentTime = time;
        SetGameTimer();
    }

    public SleepAreaDoing nowSleepObject = null;

    void SetNewDay()
    {
        if (nowSleepObject != null)
        {
            nowSleepObject.SendReadyToSkip(LocalPlayer.GetComponent<NetworkIdentity>().netId.ToString());
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (isServer)
        {
            currentTime += Time.deltaTime * timeMultyplayer;
            RpcSendTime(currentTime);
            //SetGameTimer();
        }
	}

    public void SetTime(float time)
    {
        currentTime = time;
        SetGameTimer();
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

    public enum CoockingTasja
    {
        a,
        b
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
        if (playersSkipDict.Count != PlayersDatas.Count - (removing ? 1 : 0)) 
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
        Cmd.RpcSetEnergyMultiplayer(100);
        endSkipTime = endTime;
        timeMultyplayer = mult;
        GetComponent<ObjectsScript>().botSpeedMultiplayer = 4;
    }

    void StopSkip()
    {
        timeMultyplayer = 1;
        Cmd.RpcSetEnergyMultiplayer(1);
        endSkipTime = 0;
        GetComponent<ObjectsScript>().botSpeedMultiplayer = 1;
    }

    void CreateTask(GameObject taskObject, string newTaskName, int[] stages, TaskType type, bool needButtons, bool needShowMenu)
    {
        if (!isServer) return;
        var taskController = taskObject.GetComponent<TaskControllerScript>();//cooker, digger итд
        Cmd.CmdInitTaskController(taskObject, (int)type, needShowMenu, needButtons, newTaskName, stages);
        TaskControllers.Add(taskController);
    }
}

public class PlayerInfo{
    public GameObject playerObject;
    public string nickname;
    public List<TriggerAreaDoing> currentDoings = new List<TriggerAreaDoing>();
    public GameObject inventoryData;

    public int HeadNum;
    public int BodyNum;
    public int LegsNum;

    public PlayerInfo(string nick, GameObject obj)
    {
        nickname = nick;
        playerObject = obj;
        inventoryData = obj.GetComponent<PlayerNet>().InitInventory();
    }

    public void SetBody(int head, int body, int legs)
    {
        HeadNum = head;
        BodyNum = body;
        LegsNum = legs;
    }

    public void EndAllDoins(string id)
    {
        foreach (var doing in currentDoings)
            doing.DisconectExit(id);
    }

    public void AddDoing(TriggerAreaDoing newDoing)
    {
        currentDoings.Add(newDoing);
    }

    public void RemoveDoing(TriggerAreaDoing newDoing)
    {
        currentDoings.Remove(newDoing);
    }
}
