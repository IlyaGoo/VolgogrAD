using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class MobController : StandartMoving, IAltLabelShower
{
    static readonly string[] cantSleepMessages = new string[] { "Ну и где мне спать?",
        "И куда мне лечь?", "Походу сегодня я сплю в пищевой палатке..." };

    public BotDoingCategory doingNow = BotDoingCategory.Nothing;
    public bool smoking = false;
    public Camp ownCamp = null;
    public Camp currentCamp = null;
    readonly int[] mealHeadNums = new int[] { 3, 4, 5 };
    readonly int[] femealHeadNums = new int[] { 6, 7, 8, 9 };
    readonly int[] bodyNums = new int[] { 1, 3, 4 };
    readonly int[] lagsdNums = new int[] { 0, 1 };
    public bool male;
    public string mobName = "";
    protected override bool DigIsLocal() => false;
    protected override bool FindIsController() => isServer;

    public Point CurrentPoint;
    List<Point> path = new List<Point>();
    public readonly List<BotDoing> currentDoings = new List<BotDoing>();

    public int x = -1;
    public int y = -1;
    public int z = -1;

    private GameObject _panelPr;
    public string[] startObjectsInInventory = null;

    public void ShowLabel(GameObject player) { }

    Vector3 offset = Vector3.up;
    public Vector3 Offset { get => offset; }
    public string LabelName { get => mobName; set => throw new System.NotImplementedException(); }
    public Transform TranformForPanel { get => transform; set => throw new System.NotImplementedException(); }
    public GameObject Panel
    {
        get
        {
            return _panelPr;
        }
        set
        {
            if (_panelPr != null)
            {
                dontBeReflect.Remove(_panelPr);
                Destroy(_panelPr);
            }
            _panelPr = value;
            if (_panelPr != null)
                dontBeReflect.Add(_panelPr);
        }
    }

    public void SetData(bool newMale, string newName)
    {
        mobName = newName;
        male = newMale;
    }

    public string GetCantSleepMessage()
    {
        return cantSleepMessages[UnityEngine.Random.Range(0, cantSleepMessages.Length)];
    }

    // Use this for initialization
    public void Init() {
        currentCamp = ownCamp;
        GetComponent<InventoryData>().Init();
        GetComponent<Inventory>().InitCells();
        EntitysController.instance.CreateBotData(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);
        if (!isServer)
        {
            return;
        }
        if (mobName == "")
        {
            male = UnityEngine.Random.Range(0, 2) == 0;
            mobName = mobsManager.GetRandomName(male);
        }

        var trigger = GetComponentInChildren<CircleTrigger>();
        trigger.isController = true;

        if (x == -1)
            if (male)
            {
                x = mealHeadNums[UnityEngine.Random.Range(0, mealHeadNums.Length)];
            }
            else
            {
                x = femealHeadNums[UnityEngine.Random.Range(0, femealHeadNums.Length)];
            }
        if (y == -1) y = bodyNums[UnityEngine.Random.Range(0, bodyNums.Length)];
        if (z == -1) z = lagsdNums[UnityEngine.Random.Range(0, lagsdNums.Length)];
        GetComponent<StandartMoving>().SpawnParts(x, y, z);

        if (startObjectsInInventory == null || startObjectsInInventory.Length == 0)
        {
            var r = UnityEngine.Random.Range(0, 8);
            if (r == 0)
            {
                localCommands.CmdObjectTakeItemName("TM808", 1, gameObject);
            }
            else if (r == 1)
            {
                localCommands.CmdObjectTakeItemName("XTerra505", 1, gameObject);
            }

            if (UnityEngine.Random.Range(0, 8) < 4)
                localCommands.CmdObjectTakeItemName("Lopata", 1, gameObject);
        }
        else
        {
            foreach (var item in startObjectsInInventory)
                localCommands.CmdObjectTakeItemName(item, 1, gameObject);
        }
    }

    public void InitParts(int headNum, int bodyNum, int lagsNum, bool newMale, string newName)
    {
        if (inited) return;
        mobName = newName;
        male = newMale;
        inited = true;
        SpawnParts(headNum, bodyNum, lagsNum);
    }

    void SetRandomPoint()
    {
        Point randomPoint = algo.points[UnityEngine.Random.Range(0, algo.points.Count)];
        path = algo.FindShortestPath(CurrentPoint.number, randomPoint.number);

        ClearDoings();
        foreach (var p in path)
        {
            var a = UnityEngine.Random.Range(-0.4f, 0.4f);
            AddDoing(new BotGoing(this, p.transform.position + new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), a, a / 100), p));
        }

        if (randomPoint.botsCanStay)
        {
            if(true)//UnityEngine.Random.Range(0, 5) == 1)
            {
                    //TODO курить
                AddDoing(new BotWaitingDialogue(gameObject, 20, null, randomPoint));
            }
        }

        //var a = Random.Range(-0.4f, 0.4f);
        //CurrentPosition = path[0].transform.position + new Vector3(Random.Range(-0.4f, 0.4f), a, a);
        //SetPoint(randomPoint);
    }

    public void SetPoint(Point toPoint)
    {
        CurrentPoint = toPoint;
    }

    public List<BotDoing> SetCurse(Point cursePoint)
    {
        localCommands.CmdDestroyObjectInHands(gameObject);
        path = algo.FindShortestPath(CurrentPoint.number, cursePoint.number);

        var res = new List<BotDoing>();
        foreach (var p in path)
        {
            var a = UnityEngine.Random.Range(-0.4f, 0.4f);
            res.Add(new BotGoing(this, p.transform.position + new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), a, a / 100), p));
        }
        return res;
    }

    void FixedUpdate() {
        if (!isServer || localCommands == null || stacked) return;

        if (currentDoings.Count > 0)
        {
            if (currentDoings[0].Do(Time.fixedDeltaTime))
            {
                SetPoint(currentDoings[0].needPoint);
                currentDoings.RemoveAt(0);
            }
        }
        else
        {
            doingNow = BotDoingCategory.Nothing;
            /*            if (!mobsManager.freeMobs.Contains(this))
                            mobsManager.freeMobs.Add(this);*/
            SetRandomPoint();
        }
	}  
    
    public void AddDoing(BotDoing newDoing)
    {
        currentDoings.Add(newDoing);
    }

    public void AddDoings(List<BotDoing> newDoing)
    {
        currentDoings.AddRange(newDoing);
    }

    public void ClearDoings()
    {
        if (currentDoings.Count == 0) return;
        currentDoings[0].ForceExit();
        List<BotDoing> needRemove = new List<BotDoing>();
        foreach(var doing in currentDoings)
        {
            if (doing.canHustEnd)
                needRemove.Add(doing);
        }
        foreach(var remove in needRemove)
            currentDoings.Remove(remove);
    }

    public Point GetEndPoint()
    {
        return GetLastDoing().needPoint;
    }

    public BotDoing GetLastDoing()
    {
        return currentDoings[currentDoings.Count - 1];
    }
}

abstract public class BotDoing
{
    abstract public bool Do(float deltaTime);
    abstract public void ForceExit();
    public Point needPoint;
    public bool canHustEnd = true;//Можем ли мы просто прервать задание, если false, то бот обязательно доделает это действие
    //В будущем планирую исползовать, например, при переноске дров, типа пусть бот сначала их донесет
}

public class BotMultyDoing : BotDoing//Doing с множеством других doing, когда группу дел нужно рассматривать только в совокупности
{
    readonly MobController botObject;
    TaskLogic ownLogic;
    List<BotDoing> doings = new List<BotDoing>();
    List<BotDoing> exitDoings = new List<BotDoing>();

    public BotMultyDoing(MobController bot, TaskLogic logic, Point point)
    {
        botObject = bot;
        ownLogic = logic;
        needPoint = point;
    }

    public override bool Do(float deltaTime)
    {
        if (doings.Count > 0)
        {
            if (doings[0].Do(Time.fixedDeltaTime))
            {
                botObject.SetPoint(doings[0].needPoint);
                doings.RemoveAt(0);
                if (doings.Count == 0)
                    return true;
                else
                    return false;
            }
            else return false;
        }
        else
            return false;
    }

    public void AddDoing(BotDoing newDoing, bool needExit)
    {
        doings.Add(newDoing);
        if (needExit)
            exitDoings.Add(newDoing);
    }

    public override void ForceExit()
    {
        if (doings.Count > 0)
        {
            doings[0].ForceExit();
            exitDoings.Remove(doings[0]);
        }
        foreach (var doing in exitDoings)
            doing.ForceExit();
    }
}

public class BotGoing : BotDoing//Бот как огромный мясной локомотив хуярит к определенному point-у, а точнее точке возле него
{
    protected MobController botObject;
    protected Vector3 needPosition;
    protected float distance = 0.1f;
    protected Commands _commands => MonoBehaviourExtension.localCommands;
    protected ObjectsScript objectsScripts => MonoBehaviourExtension.objectsScript;
    protected bool inited = false;
    protected bool isShifting = false;

    public BotGoing(MobController bot, Vector3 pos, Point point)
    {
        botObject = bot;
        if (bot.isShifted)
        {
            if (bot.currentShiftMultiplayer != null) bot.RemoveSpeedMultiplayer(bot.currentShiftMultiplayer);
            bot.currentShiftMultiplayer = null;
        }
        bot.isShifted = false;
        needPosition = pos;
        needPoint = point;
    }

    public void SetPos(Vector3 needPos)
    {
        needPosition = needPos;
    }

    protected virtual bool EndGoing()
    {
        return true;
    }

    protected virtual void Init()
    {}

    public override bool Do(float deltaTime)
    {
        if (!inited)
        {
            Init();
        }
        Vector3 delta = needPosition - botObject.transform.position;
        if (delta.magnitude > distance)
        {
            delta.Normalize();
            float moveSpeed = botObject.maxSpeed * deltaTime * objectsScripts.botSpeedMultiplayer * botObject.currentSpeedMultiplayer;
            botObject.transform.position = botObject.transform.position + (delta * moveSpeed);

            if (delta.x > 0)
                botObject.ChangeScale(1);
            else
                botObject.ChangeScale(-1);

            int x = delta.x > 0 ? 1 : -1;
            int y = delta.y > 0 ? 1 : -1;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) y = 0;
            else x = 0;

            _commands.CmdSetMobAnimation(botObject.gameObject, x, y, botObject.currentSpeedMultiplayer * botObject.maxSpeed / botObject.normalSpeedForAnimation, isShifting);
            return false;
        }
        else
        {
            return EndGoing();
        }
    }

    public override void ForceExit()
    {
    }
}

public class BotWaitingDialogue : BotWaiting//Бот стоит на точке и ждет разговора с кем-нибудь
{
    public BotWaitingDialogue(GameObject bot, float stayTime, string[] needAnimation, Point point, int x = -2, int y = -2):
        base(bot, stayTime, needAnimation, point, x , y)
    {
    }

    protected override void Init()
    {
        base.Init();
        needPoint.AddStayMob(botObject.GetComponent<MobController>());
    }

    public override bool Do(float deltaTime)
    {
        var res = base.Do(deltaTime);
        if (res) needPoint.RemoveStayMob(botObject.GetComponent<MobController>());
        return res;
    }

    public override void ForceExit()
    {
        base.ForceExit();
        needPoint.RemoveStayMob(botObject.GetComponent<MobController>());
    }
}

public class BotWaiting : BotDoing//Бот чилово стоит у точки, возможно держит что-то в руках
{
    protected readonly GameObject botObject;
    protected Commands localCommands => MonoBehaviourExtension.localCommands;
    public float needTime;
    string[] animated = null;
    protected bool inited;

    int needx;
    int needy;
    public Vector3 needPos = Vector3.zero;

    public BotWaiting(GameObject bot, float stayTime, string[] needAnimation, Point point, int x = -2, int y = -2)
    {
        botObject = bot;
        needTime = stayTime;
        animated = needAnimation;
        needPoint = point;
        needx = x;
        needy = y;
    }

    public void SetTimer(float newValue, bool max = false)
    {
        if (max)
            needTime = Math.Max(needTime, newValue);
        else
            needTime = newValue;
    }

    protected virtual void Init()
    {
        inited = true;
        if (animated != null)
            localCommands.CmdSpawnObjectInHands(botObject, animated);
        if (needx != -2)
        {
            localCommands.CmdSetMobAnimation(botObject, needx, needy, 0.6f, false);
            if (needx != 0)
                botObject.GetComponent<MobController>().ChangeScale(needx);
        }
    }

    public override bool Do(float deltaTime)
    {
        if (needPos != Vector3.zero)
            botObject.transform.position = needPos;

        if (!inited)
        {
            Init();
        }
        else
            localCommands.CmdSetMobAnimation(botObject, 0, 0, 0.6f, false);
        needTime -= deltaTime;

        if (IsEnd())
        {
            if (animated != null)
                localCommands.CmdDestroyObjectInHands(botObject);
            return true;
        }
        return false;
    }

    protected virtual bool IsEnd() => needTime <= 0;

    public override void ForceExit()
    {
        if (animated != null)
            localCommands.CmdDestroyObjectInHands(botObject);
    }
}

public class BotSendEnd : BotDoing//Бот говорит, я все сделал и хочу новое задание (на самом деле не хочет, и мы его заставляем)
{
    readonly MobController botObject;
    TaskLogic ownLogic;

    public BotSendEnd(MobController bot, TaskLogic logic, Point point)
    {
        botObject = bot;
        ownLogic = logic;
        needPoint = point;
    }

    public override bool Do(float deltaTime)
    {
        ownLogic.SetMobCurse(botObject);
        return false;
    }

    public override void ForceExit()
    {
    }
}

public class BotSleep : BotWaiting//Бот идет баиньки(наверно)
{
    TaskManager taskManager => MonoBehaviourExtension.taskManager;

    public BotSleep(GameObject bot, float stayTime, string[] needAnimation, Point point, int x = -2, int y = -2)
         : base(bot, stayTime, needAnimation, point, x, y)
    {
    }

    protected override void Init()
    {
        inited = true;
        var controller = botObject.GetComponent<MobController>();
        if (!controller.currentCamp.sleepArea.HaveSpace)
        {
            localCommands.CmdAddBotMessege(botObject, controller.GetCantSleepMessage(), true);
            needTime = 0;
        }
        else
            localCommands.CmdBotGoSleep(controller.GetId(), controller.currentCamp.gameObject);
        //controller.ownCamp.sleepArea.AddOneBot(botObject.gameObject);
        //controller.Panel = null;
    }

    public override void ForceExit()
    {
        var controller = botObject.GetComponent<MobController>();
        localCommands.CmdBotWakeUp(controller.GetId(), botObject.GetComponent<MobController>().currentCamp.gameObject);
    }

    protected override bool IsEnd()
    {
        var res = base.IsEnd() || Timer.instance.gameTimer >= 421;
        if (res) ForceExit();
        return res;
    }

}

public class BotGoingInZone : BotGoing//Бот чилово гуляет в зоне, возможно держит что-то в руках
{
    Vector3 point1;
    Vector3 point2;

    protected override void Init()
    {
        if (!botObject.isShifted)
        {
            if (botObject.currentShiftMultiplayer != null) botObject.RemoveSpeedMultiplayer(botObject.currentShiftMultiplayer);
            botObject.currentShiftMultiplayer = botObject.AddSpeedMultiplayer(botObject.GetShiftMultiplayer());
        }
        //botObject.isShifted = true; //Так нельзя делать, потому что не включится металлодетектор, кидаем true в Do
        isShifting = true;
    }

    public BotGoingInZone(MobController bot, Vector3 pos, Point point, (Vector3, Vector3) points, string[] objectName = null)
        : base(bot, pos, point)
    {
        point1 = points.Item1;
        point2 = points.Item2;
        SetRandomPoint();

        if (objectName != null)
        {
            _commands.CmdSpawnObjectInHands(botObject.gameObject, objectName);
        }
    }

    void SetRandomPoint()
    {
        var y = UnityEngine.Random.Range(point1.y, point2.y);
        needPosition = new Vector3(UnityEngine.Random.Range(point1.x, point2.x), y, y / 100);
    }

    protected override bool EndGoing()
    {
        SetRandomPoint();
        return false;
    }
}

public enum BotDoingCategory//Чтобы понимать что бот вообще сейчас делает
{
    Sleeping,
    Nothing,
    Dialogue,
    Digging,
    Coocking,
    Postr
}