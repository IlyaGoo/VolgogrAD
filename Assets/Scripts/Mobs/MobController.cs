using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MobController : StandartMoving, IAltLabelShower
{
    readonly int[] mealHeadNums = new int[] { 3, 4, 5 };
    readonly int[] femealHeadNums = new int[] { 6, 7, 8, 9 };
    readonly int[] bodyNums = new int[] { 1, 3, 4 };
    readonly int[] lagsdNums = new int[] { 0, 1 };
    public bool male;
    public string mobName = "";
    public override PlayerNet PlNet() => manager.playNet;
    protected override bool DigIsLocal() => false;
    protected override bool FindIsController() => isServer;

    public Point CurrentPoint;
    List<Point> path = new List<Point>();
    Algo Algoritm;
    public Commands _commands;
    ObjectsScript constants;

    public List<BotDoing> currentDoings = new List<BotDoing>();

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

    MobsManager manager;

    public void SetData(bool newMale, string newName)
    {
        mobName = newName;
        male = newMale;
    }

    // Use this for initialization
    public void Init(MobsManager man) {
        GetComponent<InventoryData>().Init();
        GetComponent<Inventory>().InitCells();
        manager = man;
        _commands = manager.cmd;
        constants = manager._taskManager.objectsScript;
        if (!isServer)
        {
            return;
        }

        if (mobName == "")
        {
            male = UnityEngine.Random.Range(0, 2) == 0;
            mobName = manager.GetRandomName(male);
        }

        var trigger = GetComponentInChildren<CircleTrigger>();
        trigger.cmd = _commands;
        trigger.isController = true;

        Algoritm = manager.algoritm;
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
                _commands.CmdObjectTakeItemName("TM808", 1, gameObject);
            }
            else if (r == 1)
            {
                _commands.CmdObjectTakeItemName("XTerra505", 1, gameObject);
            }

            if (UnityEngine.Random.Range(0, 8) < 4)
                _commands.CmdObjectTakeItemName("Lopata", 1, gameObject);
        }
        else
        {
            foreach (var item in startObjectsInInventory)
                _commands.CmdObjectTakeItemName(item, 1, gameObject);
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
        Point randomPoint = Algoritm.points[UnityEngine.Random.Range(0, Algoritm.points.Count)];
        path = Algoritm.FindShortestPath(CurrentPoint.number, randomPoint.number);

        currentDoings.Clear();
        foreach (var p in path)
        {
            var a = UnityEngine.Random.Range(-0.4f, 0.4f);
            currentDoings.Add(new BotGoing(this, p.transform.position + new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), a, a / 100), p, constants));
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
        _commands.CmdDestroyObjectInHands(gameObject);
        path = Algoritm.FindShortestPath(CurrentPoint.number, cursePoint.number);

        var res = new List<BotDoing>();
        foreach (var p in path)
        {
            var a = UnityEngine.Random.Range(-0.4f, 0.4f);
            res.Add(new BotGoing(this, p.transform.position + new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), a, a / 100), p, constants));
        }
        return res;


    }

    void FixedUpdate() {
        if (!isServer || _commands == null || stacked) return;

        if (currentDoings.Count > 0)
        {
            if (currentDoings[0].Do(Time.fixedDeltaTime))
            {
                SetPoint(currentDoings[0].needPoint);
                currentDoings.RemoveAt(0);
            }
        }
        else SetRandomPoint();
	}    
}

abstract public class BotDoing
{
    abstract public bool Do(float deltaTime);
    public Point needPoint;
}

public class BotGoing : BotDoing
{
    protected MobController botObject;
    protected Vector3 needPosition;
    protected float distance = 0.1f;
    protected float speed = 3;
    protected Commands _commands;
    protected ObjectsScript constants;
    protected float shiftMultiplayer = 0.47f;
    protected bool isNowShifted = false;

    public BotGoing(MobController bot, Vector3 pos, Point point, ObjectsScript con)
    {
        botObject = bot;
        needPosition = pos;
        _commands = bot._commands;
        needPoint = point;
        constants = con;
    }

    public void SetPos(Vector3 needPos)
    {
        needPosition = needPos;
    }

    protected virtual bool EndGoing()
    {
        return true;
    }

    public override bool Do(float deltaTime)
    {
        Vector3 delta = needPosition - botObject.transform.position;
        if (delta.magnitude > distance)
        {
            delta.Normalize();
            float moveSpeed = speed * deltaTime * constants.botSpeedMultiplayer * shiftMultiplayer;
            botObject.transform.position = botObject.transform.position + (delta * moveSpeed);

            if (delta.x > 0)
                botObject.ChangeScale(1);
            else
                botObject.ChangeScale(-1);

            int x = delta.x > 0 ? 1 : -1;
            int y = delta.y > 0 ? 1 : -1;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) y = 0;
            else x = 0;

            _commands.CmdSetMobAnimation(botObject.gameObject, x, y, shiftMultiplayer, isNowShifted);
            return false;
        }
        else
        {
            return EndGoing();
        }
    }
}

public class BotWaiting : BotDoing
{
    readonly GameObject botObject;
    Commands cmd;
    float needTime;
    bool animated;
    bool inited;

    int needx;
    int needy;
    public Vector3 needPos = Vector3.zero;

    public BotWaiting(GameObject bot, float stayTime, Commands com, bool needAnimation, Point point, int x = -2, int y = -2)
    {
        botObject = bot;
        needTime = stayTime;
        cmd = com;
        animated = needAnimation;
        needPoint = point;
        needx = x;
        needy = y;
    }

    public override bool Do(float deltaTime)
    {
        if (needPos != Vector3.zero)
            botObject.transform.position = needPos;

        if (!inited)
        {
            inited = true;
            if (animated)
                cmd.CmdSpawnObjectInHands(botObject, new string[1] { "Doing" });
            if (needx != -2)
            {
                cmd.CmdSetMobAnimation(botObject, needx, needy, 0.6f, false);
                if (needx != 0)
                    botObject.GetComponent<MobController>().ChangeScale(needx);
            }
        }
        else
            cmd.CmdSetMobAnimation(botObject, 0, 0, 0.6f, false);
        needTime -= deltaTime;

        if (needTime <= 0)
        {
            if (animated)
                cmd.CmdDestroyObjectInHands(botObject);
            return true;
        }
        return false;
    }
}

public class BotSendEnd : BotDoing
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
}

public class BotGoingInZone : BotGoing
{
    Vector3 point1;
    Vector3 point2;

    public BotGoingInZone(MobController bot, Vector3 pos, Point point, ObjectsScript con, Vector3 p1, Vector3 p2, string[] objectName = null)
        : base(bot, pos, point, con)
    {
        isNowShifted = true;
        shiftMultiplayer = 0.35f;
        point1 = p1;
        point2 = p2;
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