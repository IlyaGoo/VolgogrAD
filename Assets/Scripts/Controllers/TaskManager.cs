using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

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

    private bool _inited;

    /** Стандартный шаблон на день**/
    public List<TaskInfo> standartDayTemplate;
    /** Таски, которые выполняются прямо сейчас**/
    public List<TaskInfo> currentTasks = new List<TaskInfo>();
    /** Таски, которые осталось выполнить в этот день**/
    public List<TaskInfo> remainingTasks = new List<TaskInfo>();
    
    /** Стандартный шаблон каких-то событий на день, типа затемнения светящихся объектов итд**/
    private static readonly List<AbstractTimeDoing> StandardDayTimeDoingTemplate = new List<AbstractTimeDoing>
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
    private List<AbstractTimeDoing> _remainingTimeDoings = new List<AbstractTimeDoing>();
    /** Действия, которые нужно делать в какое-то время**/
    private readonly List<AbstractTimeDoing> _currentTimeDoings = new List<AbstractTimeDoing>();
    
    private TaskManager()
    { }
    
    private void InitTemplates()
    {
        standartDayTemplate = new List<TaskInfo>
        {
            new TaskInfo("Quest", "Сон", TaskType.Sleeping, false, false, 0, 420, true),
            new TaskInfo("Quest", "Приготовить завтрак", TaskType.Cooking, true, true, 420, 600, false, new StarQuestObjectGetter(CampObjectType.Kitchen)),
            new TaskInfo("Quest", "Построение", TaskType.Standing, false, false, 600, 630, true),
            new TaskInfo("Quest", "Раскопки", TaskType.Digging, false, false, 630, 870, false),
            new TaskInfo("Quest", "Приготовить обед", TaskType.Cooking, true, true, 720, 900, false, new StarQuestObjectGetter(CampObjectType.Kitchen)),
            new TaskInfo("Quest", "Раскопки", TaskType.Digging, false, false, 990, 1170, false),
            new TaskInfo("Quest", "Приготовить ужин", TaskType.Cooking, true, true, 1020, 1200, false, new StarQuestObjectGetter(CampObjectType.Kitchen))
        };
    }

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        InitTemplates();
        SetDayDoingsTemplate();
        _inited = true;
        
        SetDayTasksTemplate();
    }

    public void CheckEndStartTasks()
    {
        var needRemoveFromCurrentTasks = new List<TaskInfo>();
        var needRemoveFromRemainingTasks = new List<TaskInfo>();
        
        foreach (var task in currentTasks.Where(task => Timer.instance.gameTimer >= task.endTime))//Сначала заканчиваем таски, вдруг новый таск будет на том же месте
        {
            //localCommands.RpcEndTaskController(task.currentScript.gameObject);
            if (isServer) task.EndQuest();
            needRemoveFromCurrentTasks.Add(task);
        }
        
        foreach (var task in needRemoveFromCurrentTasks)
            currentTasks.Remove(task);
        
        foreach (var task in remainingTasks.Where(task => Timer.instance.gameTimer >= task.startTime))
        {
            if (isServer) task.StartQuest();
            // localCommands.RpcReinitializeQuestController(
            //     task.Controller.gameObject, 
            //     task.TaskName,
            //     task.NeedStartAfterCreate
            //     );
            currentTasks.Add(task);
            needRemoveFromRemainingTasks.Add(task);
        }

        foreach (var task in needRemoveFromRemainingTasks)
            remainingTasks.Remove(task);
    }
    
    public void CheckEndStartTimeDoings()
    {
        var needRemoveFromCurrentTimeDoings = new List<AbstractTimeDoing>();
        var needRemoveFromRemainingTimeDoings = new List<AbstractTimeDoing>();
        
        foreach (var task in _remainingTimeDoings.Where(task => Timer.instance.gameTimer >= task.startTime))
        {
            _currentTimeDoings.Add(task);
            needRemoveFromRemainingTimeDoings.Add(task);
        }

        foreach (var task in _currentTimeDoings.Where(task => Timer.instance.gameTimer >= task.endTime))
        {
            needRemoveFromCurrentTimeDoings.Add(task);
        }

        foreach (var doing in _currentTimeDoings)
            doing.Do();

        foreach (var task in needRemoveFromCurrentTimeDoings)
            _currentTimeDoings.Remove(task);
        foreach (var task in needRemoveFromRemainingTimeDoings)
            _remainingTimeDoings.Remove(task);
    }

    public void SetDayTasksTemplate()
    {
        remainingTasks = new List<TaskInfo>(standartDayTemplate); //Строим текущий план на день
        currentTasks.Clear();//todo веротяно с появление норм квестов, должно отлететь
    }
    
    public void SetDayDoingsTemplate()
    {
        _remainingTimeDoings = new List<AbstractTimeDoing>(StandardDayTimeDoingTemplate); //Строим текущий план действий на день
        _currentTimeDoings.Clear();
    }
}