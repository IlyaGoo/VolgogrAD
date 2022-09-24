using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobsManager : MonoBehaviourExtension
{
    public static MobsManager instance;
    public List<MobController> mobs = new List<MobController>();
    public List<MobController> freeMobs;
    List<TaskLogic> currentTasks = new List<TaskLogic>();
    List<TaskLogic> taskReturns = new List<TaskLogic>();
    bool isServer;
    [SerializeField] TextAsset maleText;
    [SerializeField] TextAsset femaleText;

    List<string> maleNames = new List<string>();
    List<string> femaleNames = new List<string>();

    private MobsManager()
    { }

    private void Awake()
    {
        instance = this;
        ReadNames();
    }

    public void Init()
    {
        isServer = taskManager.isServer;
        SetMobs();//Потом, когда будем спавнить мобов, возможно, нужно будет вызывать после спавна, а не нас старте
        foreach (var mob in mobs)
        {
            mob.Init();
        }
    }
    
    public void SendAllBotsToSleep()
    {
        foreach(var mob in mobs)
        {
            if (mob.currentCamp != null && 
                mob.currentCamp.sleepArea != null && 
                !mob.currentCamp.sleepArea.sleepers.Exists(info => info.Identity == mob.GetId()) &&
                mob.currentCamp.sleepArea.HaveSpace
                )
            {
                mob.ClearDoings();
                mob.CurrentPoint = mob.currentCamp.ownPoint;
                mob.SetAllRenders(false);
                mob.transform.position = mob.currentCamp.sleepArea.transform.position;
                mob.AddDoing(new BotSleep(mob.gameObject, 100000, null, mob.ownCamp.ownPoint));
            }
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
            return maleNames[UnityEngine.Random.Range(0, maleNames.Count)];
        else
            return femaleNames[UnityEngine.Random.Range(0, femaleNames.Count)];
    }

    public void SetMobs()
    {
        foreach (Transform child in transform)
        {
            mobs.Add(child.GetComponent<MobController>());
        }
        freeMobs = new List<MobController>(mobs);
    }

    public TaskLogic AddTask(QuestController newTask)
    {
        var newTaskLogic = new TaskLogic(newTask);

        if (!taskManager.isServer) return newTaskLogic;

        currentTasks.Add(newTaskLogic);
        RefillTasks(newTaskLogic);
        //PrintTasks();
        //if (newTask.canReturnMobs) taskReturns.Add(newTaskLogic);
        return newTaskLogic;
    }

    void RefillTasks(TaskLogic newTask)//Общий перерасчет ботов по таскам
    {
        int allreturnableAndFreeBots = freeMobs.Count;
        int allMaxSum = newTask.maxCount;
        foreach (var task in taskReturns)
        {
            allreturnableAndFreeBots += task.taskMobs.Count;
            allMaxSum += task.maxCount;
        }

        foreach (var task in taskReturns)
        {
            var needReturn = Mathf.Min(task.taskMobs.Count - 1, (int)(task.taskMobs.Count - (float)task.maxCount / allMaxSum * allreturnableAndFreeBots));
            if (needReturn <= 0) continue;
            var returnableMobs = TakeFromList(task.taskMobs, needReturn);
            freeMobs.AddRange(returnableMobs);
            foreach (var mob in returnableMobs)
            {
                mob.ClearDoings();
                localCommands.CmdDestroyObjectInHands(mob.gameObject);
            }
        }

        foreach (var task in taskReturns)
        {
            AddReFillingtask(task, allMaxSum, allreturnableAndFreeBots);
        }

        AddReFillingtask(newTask, allMaxSum, allreturnableAndFreeBots);

        CheckLastMob(newTask);

    }

    void AddReFillingtask(TaskLogic task, int allMaxSum, int allreturnableAndFreeBots)
    {
        var needAdd = Mathf.Max(task.taskMobs.Count == 0 ? 1 : 0, (int)((float)task.maxCount / allMaxSum * allreturnableAndFreeBots - task.taskMobs.Count));
        if (needAdd <= 0) return;
        task.AddMobs(TakeFromList(freeMobs, needAdd));
    }

    void CheckCanFillTasks()//Вызываем когда освободились свободные боты
    {
        foreach (var task in currentTasks)
            if (task.taskMobs.Count == 0 && freeMobs.Count > 0)
                task.AddMobs(TakeFromList(freeMobs, 1));

        int allNeed = 0;
        foreach(var task in currentTasks)
        {
            allNeed += task.NeedMobsCount;
        }
        if (allNeed == 0) return;
        foreach(var task in currentTasks)
        {
            task.AddMobs(TakeFromList(freeMobs, Mathf.Min(task.NeedMobsCount, (int)((float)task.NeedMobsCount / allNeed * freeMobs.Count))));
        }

        CheckLastMob();
    }

    void CheckLastMob(TaskLogic newTask = null)
    {//Бывает, что одного бота мы недораспределяем из-за целочисленного деление
        if (freeMobs.Count == 1)
        {
            if (newTask != null && newTask.NeedMobsCount > 0)
            {
                newTask.AddMobs(TakeFromList(freeMobs, 1));
                return;
            }
            foreach (var task in currentTasks)
                if (task.NeedMobsCount > 0)
                {
                    task.AddMobs(TakeFromList(freeMobs, 1));
                    break;
                }
        }
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
        if(count > 0)
            for (var i = 0; i < count; i++)
            {
                var index = UnityEngine.Random.Range(0, list.Count);
                result.Add(list[index]);
                list.RemoveAt(index);
                if (list.Count == 0) break;
            }
        return result;
    }

    public void RemoveTask(TaskLogic needRemove)
    {
        if (!taskManager.isServer) return;
        currentTasks.Remove(needRemove);
        taskReturns.Remove(needRemove);

        freeMobs.AddRange(needRemove.taskMobs);
        if (needRemove.needFreeMobsAfterEnd)
        {
            foreach (var mob in needRemove.taskMobs)
            {
                mob.ClearDoings();
                localCommands.CmdDestroyObjectInHands(mob.gameObject);
            }
        }
        CheckCanFillTasks();//Освободились боты и мы пытаемся отправить их куда-нибудь еще
    }
}

public class TaskLogic
{
    public GameObject taskObject;
    public bool needFreeMobsAfterEnd;//Иногда она сначала должны доделать что-то, например поспать, ни в коем случае в конце не делать botsendend, потому что это зациклит бота
    public bool canReturn = true;
    public int maxCount = 10;
    public List<MobController> taskMobs = new List<MobController>();
    public QuestStep[] controllers;
    QuestController _taskController;
    ObjectsScript ObjectsContainer => MonoBehaviourExtension.objectsScript;
    public int NeedMobsCount => Mathf.Max(0, maxCount - taskMobs.Count);

    public TaskLogic(QuestController controller)
    {
        // needFreeMobsAfterEnd = controller.needFreeMobsAfterEnd;
        // maxCount = controller.needBotsCount;
        // _taskController = controller;
        // controllers = controller._minigame;
    }

    public void End()
    {
        switch (_taskController.taskType)
        {
            case TaskType.Cooking:
                
                break;
            case TaskType.Standing:
                _taskController.GetComponent<PostrController>().ReInitPos();
                break;
        }
    }

    public void AddMobs(List<MobController> newMob)
    {
        taskMobs.AddRange(newMob);

        if(_taskController.taskType == TaskType.Digging)//Равномерно распределяем ботов с металлодетекторами
        {
            int needMobsWithDet = Mathf.Max(1, newMob.Count / 3);
            List<BotInstructions> botsWithMetallodetecor = new List<BotInstructions>();//Список ботов с металиком и лопатой одновременно
            List<BotInstructions> botInstructions = new List<BotInstructions>();//Конечный список инструкций, который кинем в setCurse
            foreach (var bot in newMob)
            {
                var inv = bot.GetComponent<Inventory>();
                var met = inv.HasInCategory(ItemCategory.MetalDetector);
                var lop = inv.HasIn("Lopata");
                if (met != null)
                {
                    if (lop == null)//Кидаем в конечный список ботов только с металиком
                    {
                        var newInstruction = new BotInstructions(bot)
                        {
                            detector = met
                        };
                        botInstructions.Add(newInstruction);
                        needMobsWithDet--;
                    }
                    else
                    {
                        botsWithMetallodetecor.Add(new BotInstructions(bot)
                        {
                            detector = met,
                            lopata = lop
                        }
                        );
                    }
                }
                else //if (lop != null)//Кидаем в конечный список ботов только с лопатой
                {
                    var newInstruction = new BotInstructions(bot)
                    {
                        lopata = lop
                    };
                    botInstructions.Add(newInstruction);
                }
            }

            while(needMobsWithDet > 0 && botsWithMetallodetecor.Count > 0)//Нужно оптимизировать и складывать ботов сразу в порядке возрастания качества металика
            {
                needMobsWithDet--;
                BotInstructions botWithBestDet = botsWithMetallodetecor[0];
                foreach(var bot in botsWithMetallodetecor)//Ищем лучший металлодетектор
                {
                    if(bot.detector.itemQality > botWithBestDet.detector.itemQality)
                    {
                        botWithBestDet = bot;
                    }
                }
                botInstructions.Add(botWithBestDet);
                botsWithMetallodetecor.Remove(botWithBestDet);
            }
            foreach(var lopBot in botsWithMetallodetecor)//У оставшихся убираем металик, пусть ходят только с лопатой
            {
                lopBot.detector = null;
                botInstructions.Add(lopBot);
            }

            foreach (var instruction in botInstructions)
            {
                SetMobCurse(instruction.cont, instruction);
            }
        }

        else foreach(var mob in newMob)
        {
            //mob.SetCurse();
            SetMobCurse(mob);
        }
    }

    public class BotInstructions
    {
        public BotInstructions(MobController contr)
        {
            cont = contr;
        }

        public MobController cont = null;
        public ItemData detector = null;
        public ItemData lopata = null;
    }

    public void SetMobCurse(MobController con, BotInstructions instructions = null)
    {
        switch (_taskController.taskType)
        {
            case TaskType.Cooking:
                con.ClearDoings();
                con.doingNow = BotDoingCategory.Coocking;
                con.AddDoings(con.SetCurse(controllers[UnityEngine.Random.Range(0, controllers.Length)].currentCampObject.ownPoint));

                if (UnityEngine.Random.Range(0, 2) == 0)
                    con.AddDoing(new BotWaiting(con.gameObject, UnityEngine.Random.Range(20, 100), UnityEngine.Random.Range(0, 2) == 0 ? new string[1] { "Doing" } : null, con.GetEndPoint()));
                con.AddDoing(new BotSendEnd(con, this, con.GetEndPoint()));
                break;
            case TaskType.Sleeping:
                con.ClearDoings();
                con.doingNow = BotDoingCategory.Sleeping;
                if (con.ownCamp != null)
                {
                    con.AddDoings(con.SetCurse(con.currentCamp.ownPoint));
                    con.AddDoing(new BotGoing(con, con.currentCamp.sleepArea.transform.position, con.currentCamp.ownPoint));
                    con.AddDoing(new BotSleep(con.gameObject, 100000, null, con.currentCamp.ownPoint));
                }
                else
                    MonoBehaviourExtension.localCommands.CmdAddBotMessege(con.gameObject, con.GetCantSleepMessage(), true);
                break;
            case TaskType.Standing:
                if (con.oldLevel > 5) break;//Если моб достаточно олд, то он ебет в рот ваше построение
                con.ClearDoings();
                con.doingNow = BotDoingCategory.Postr;
                con.AddDoings(con.SetCurse(controllers[UnityEngine.Random.Range(0, controllers.Length)].currentCampObject.ownPoint));

                var pos = _taskController.GetComponent<PostrController>().AddOne();
                ((BotGoing)con.GetLastDoing()).SetPos(pos);

                var wait = new BotWaiting(con.gameObject, 100000, null, con.GetEndPoint(), 1, 0)
                {
                    needPos = pos
                };
                con.AddDoing(wait);
                con.AddDoing(new BotSendEnd(con, this, con.GetEndPoint()));
                break;
            case TaskType.Digging:
                con.ClearDoings();
                con.doingNow = BotDoingCategory.Digging;
                con.AddDoings(con.SetCurse(controllers[UnityEngine.Random.Range(0, controllers.Length)].currentCampObject.ownPoint));
                ItemData lop = null;
                List<string> inHandsNames = null;
                string detectorName = null;//Нужно чтобы потом если что поменять положение в руках
                if (instructions == null)//Если нет инструкции, то просто берем все самое лучшее, что у нас есть
                {
                    var detectorData = con.GetComponent<Inventory>().HasInCategory(ItemCategory.MetalDetector);
                    if (detectorData != null)
                    {
                        inHandsNames = new List<string>(detectorData.GetMobsInHands());
                        if (!detectorData.twoHanded)
                        {
                            lop = con.GetComponent<Inventory>().HasIn("Lopata");
                            if (lop != null)
                            {
                                detectorName = inHandsNames[1];
                                inHandsNames.AddRange(lop.inHandsPrefab);
                            }
                        }
                    }
                    else if ((detectorData = con.GetComponent<Inventory>().HasIn("Lopata")) != null)
                    {
                        inHandsNames = new List<string>(detectorData.inHandsPrefab);
                    }
                }
                else
                {
                    inHandsNames = new List<string>();
                    lop = instructions.lopata;
                    if (instructions.detector!= null)
                        detectorName = instructions.detector.inHandsPrefab[1];
                    if (instructions.detector != null)
                    {
                        inHandsNames.AddRange(instructions.detector.GetMobsInHands());
                        if (instructions.detector.twoHanded)
                            lop = null;
                    }
                    if (lop != null)
                    {
                        inHandsNames.AddRange(lop.inHandsPrefab);
                    }
                }
                var needPoint = con.GetEndPoint();
                //con.AddDoing(new BotGoingInZone(con, needPoint.transform.position, needPoint, _taskController.GetComponent<DiggingController>().GetPoints(), inHandsNames?.ToArray()));
                if (lop != null && detectorName != null)
                {
                    MonoBehaviourExtension.localCommands.CmdChangeInHandsPosition(con.gameObject, detectorName, true);
                }
                con.AddDoing(new BotSendEnd(con, this, con.GetEndPoint()));
                break;
        }
    }
}
