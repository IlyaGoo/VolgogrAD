using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobsManager : MonoBehaviour
{

    public List<MobController> mobs = new List<MobController>();
    List<MobController> freeMobs;
    public TaskManager _taskManager;
    public Algo algoritm;
    ObjectsScript Constants => _taskManager.objectsScript;
    public Commands cmd;
    public PlayerNet playNet;
    List<TaskLogic> currentTasks = new List<TaskLogic>();
    List<TaskLogic> taskReturns = new List<TaskLogic>();
    bool isServer;
    [SerializeField] TextAsset maleText;
    [SerializeField] TextAsset femaleText;

    List<string> maleNames = new List<string>();
    List<string> femaleNames = new List<string>();

    // Use this for initialization
    void Start() {
        isServer = _taskManager.isServer;
        SetMobs();//Потом, когда будем спавнить мобов, возможно, нужно будет вызывать после спавна, а не нас старте
        ReadNames();
    }

    public void Init(GameObject player, PlayerNet plNet)
    {
        cmd = player.GetComponent<Commands>();
        playNet = plNet;
        foreach (var mob in mobs)
        {
            mob.Init(this);
        }
    }

    void ReadNames()
    {
        maleNames = new List<string>(maleText.text.Split('\n'));
        femaleNames = new List<string>(femaleText.text.Split('\n'));
    }

    public string GetRandomName(bool male)
    {
        if (maleNames.Count == 0)
            ReadNames();
        if (male)
            return maleNames[Random.Range(0, maleNames.Count)];
        else
            return femaleNames[Random.Range(0, femaleNames.Count)];
    }

    public void SetMobs()
    {
        foreach (Transform child in transform)
        {
            mobs.Add(child.GetComponent<MobController>());
        }
        freeMobs = new List<MobController>(mobs);
    }

    public TaskLogic AddTask(TaskControllerScript newTask)
    {
        var newTaskLogic = new TaskLogic(newTask);

        if (!_taskManager.isServer) return newTaskLogic;

        currentTasks.Add(newTaskLogic);

        var needCount = newTaskLogic.maxCount;
        var i = Mathf.Min(needCount,freeMobs.Count);
        newTaskLogic.AddMobs(TakeFromList(freeMobs, i));
        needCount -= i;
        if (needCount == newTaskLogic.maxCount)
            foreach(var task in taskReturns)
            {
                newTaskLogic.AddMobs(TakeFromList(task.taskMobs, Mathf.Min(1, task.taskMobs.Count)));
            }
        //PrintTasks();
        taskReturns.Add(newTaskLogic);
        return newTaskLogic;
    }

    void PrintTasks()
    {
        foreach(var t in currentTasks)
        {
            print(t.taskMobs.Count);
        }
    }

    List<MobController> TakeFromList(List<MobController> list, int count)
    {
        var result = new List<MobController>();
        for (var i = 0; i < count; i++)
        {
            var index = Random.Range(0, list.Count);
            result.Add(list[index]);
            list.RemoveAt(index);
        }
        return result;
    }

    public void RemoveTask(TaskLogic needRemove)
    {
        if (!_taskManager.isServer) return;
        currentTasks.Remove(needRemove);
        taskReturns.Remove(needRemove);

        freeMobs.AddRange(needRemove.taskMobs);

        foreach(var mob in needRemove.taskMobs)
        {
            mob.currentDoings.Clear();
            cmd.CmdDestroyObjectInHands(mob.gameObject);
        }
    }
}

public class TaskLogic
{
    public GameObject taskObject;
    public bool canReturn = true;
    public int maxCount = 10;
    public List<MobController> taskMobs = new List<MobController>();
    public MiniGameController[] controllers;
    TaskControllerScript _taskController;
    ObjectsScript Constants => _taskController._mobManager._taskManager.objectsScript;

    public TaskLogic(TaskControllerScript controller)
    {
        _taskController = controller;
        controllers = controller._minigame;
    }

    public void End()
    {
        switch (_taskController.currentType)
        {
            case TaskType.Coocking:
                
                break;
            case TaskType.Standing:
                _taskController.GetComponent<PostrController>().ReInitPos();
                break;
        }
    }

    public void AddMobs(List<MobController> newMob)
    {
        taskMobs.AddRange(newMob);
        foreach(var mob in newMob)
        {
            //mob.SetCurse();
            SetMobCurse(mob);
        }
    }

    public void SetMobCurse(MobController con)
    {
        switch (_taskController.currentType)
        {
            case TaskType.Coocking:
                con.currentDoings.Clear();
                con.currentDoings.AddRange(con.SetCurse(controllers[Random.Range(0, controllers.Length)].ownPoint));

                if (Random.Range(0, 2) == 0)
                    con.currentDoings.Add(new BotWaiting(con.gameObject, Random.Range(20, 100), con._commands, Random.Range(0, 2) == 0, con.currentDoings[con.currentDoings.Count - 1].needPoint));
                con.currentDoings.Add(new BotSendEnd(con, this, con.currentDoings[con.currentDoings.Count - 1].needPoint));
                break;
            case TaskType.Standing:
                con.currentDoings.Clear();
                con.currentDoings.AddRange(con.SetCurse(controllers[Random.Range(0, controllers.Length)].ownPoint));

                var pos = _taskController.GetComponent<PostrController>().AddOne();
                ((BotGoing)con.currentDoings[con.currentDoings.Count - 1]).SetPos(pos);


                var wait = new BotWaiting(con.gameObject, 100000, con._commands, false, con.currentDoings[con.currentDoings.Count - 1].needPoint, 1, 0);
                wait.needPos = pos;
                con.currentDoings.Add(wait);
                con.currentDoings.Add(new BotSendEnd(con, this, con.currentDoings[con.currentDoings.Count - 1].needPoint));
                break;
            case TaskType.Diging:
                con.currentDoings.Clear();
                con.currentDoings.AddRange(con.SetCurse(controllers[Random.Range(0, controllers.Length)].ownPoint));

                var size = controllers[0].size;
                var offset = controllers[0].offset;
                var y1 = controllers[0].transform.position.y - size.y / 2 + offset.y;
                var y2 = controllers[0].transform.position.y + size.y / 2 - offset.y;
                var p1 = new Vector3(controllers[0].transform.position.x - size.x/2 + offset.x, y1, y1);
                var p2 = new Vector3(controllers[0].transform.position.x + size.x / 2 - offset.x, y2, y2);

                ItemData lop = null;
                List<string> inHandsPrefabs = null;
                string detector = null;
                var inHands = con.GetComponent<Inventory>().HasIn("Ramka");
                if (inHands == null)
                {
                    inHands = con.GetComponent<Inventory>().HasIn("TM808");
                    if (inHands == null)
                        inHands = con.GetComponent<Inventory>().HasIn("XTerra505");
                }
                if (inHands != null)
                {
                    inHandsPrefabs = new List<string>(inHands.inHandsPrefab);
                    if (inHandsPrefabs[1].Equals("ramkaInHands"))
                        inHandsPrefabs[1] = "ramkaBotsInHands";
                    lop = con.GetComponent<Inventory>().HasIn("Lopata");
                    if (lop != null)
                    {
                        detector = inHandsPrefabs[1];
                        inHandsPrefabs.AddRange(lop.inHandsPrefab);
                    }
                }
                else if ((inHands = con.GetComponent<Inventory>().HasIn("Lopata")) != null)
                {
                    inHandsPrefabs = new List<string>(inHands.inHandsPrefab);
                }

                //string[] inHands = taskMobs.IndexOf(con) == 0 ? new string[2] { "FindCircle1", "GarretInHands" } : taskMobs.IndexOf(con) == 1 ? new string[2] { "FindCircle3", "TM808InHands" } : null;
                var needPoint = con.currentDoings[con.currentDoings.Count - 1].needPoint;
                con.currentDoings.Add(new BotGoingInZone(con, needPoint.transform.position, needPoint, Constants, p1, p2, inHandsPrefabs?.ToArray()));
                //new BotGoing(this, p.transform.position +new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), a, a / 100), p, Constants)
                if (lop != null)
                {
                    con._commands.CmdChangeInHandsPosition(con.gameObject, detector, true);
                }
                con.currentDoings.Add(new BotSendEnd(con, this, con.currentDoings[con.currentDoings.Count - 1].needPoint));
                break;
        }
    }
}
