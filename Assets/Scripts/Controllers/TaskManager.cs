using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
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

    /** Стандартный шаблон на день */
    public List<TaskInfo> standartDayTemplate;
    /** Таски, которые выполняются прямо сейчас */
    [FormerlySerializedAs("currentTasks")] public List<QuestController> currentQuests = new();
    /** Таски, которые осталось выполнить в этот день */
    public List<TaskInfo> remainingTasks = new();
    
    /** Стандартный шаблон каких-то событий на день, типа затемнения светящихся объектов итд */
    private static readonly List<AbstractTimeDoing> StandardDayTimeDoingTemplate = new()
    {
        new SetLightObjectsTimeDoing(420, false, false),
        new SetLightObjectsTimeDoing(1230, true, false),
        new SetLightLevelOnce(0, 0, 1, false),
        new SetLightLevelOnce(420, 0.8f, 0, false),
        new SendBotsToSleepTimeDoing(220, true),
        new SetLightLevelUntilMidnightTimeDoing(1230, 1440, false),
        new SetLightLevelAfterMidnightTimeDoing(220, 420, false)
    };
    /** Действия, которые нужно еще сделать в какое-то время */
    private List<AbstractTimeDoing> _remainingTimeDoings = new();
    /** Действия, которые нужно делать в какое-то время */
    private readonly List<AbstractTimeDoing> _currentTimeDoings = new();
    
    private TaskManager()
    { }

    public QuestController _targetQuest;//Текущий выбранный квест
    public QuestStep _targetStep;//Текущий выбранный шаг квеста, является более приоритетным

    private IQuestTarget GetTarget()
    {
        if (_targetStep != null) return _targetStep;
        return _targetQuest;
    }

    public Transform GetTargetTransform()
    {
        return GetTarget()?.GetTargetTransform();
    }

    /** Сбросить активность с текущего квеста */
    private void ClearQuestState()
    {
        if (_targetQuest != null)
        {
            QuestPanel.ChangeQuestButtonState(_targetQuest, false);
            _targetQuest = null;
            
            ClearStepState();
        }
    }
    
    /** Сбросить активность с текущего шага */
    private void ClearStepState()
    {
        if (_targetStep != null)
        {
            QuestPanel.ChangeStepButtonState(_targetStep, false);
            var targetedType = _targetStep.needObjectType;
            _targetStep = null;
            MapScript.Instance.UpdateAllSteps(_targetQuest);
            localTaker.UpdateCampTriggerArea(targetedType);
        }
    }
    
    /** Обновить состояние квеста */
    public void UpdateTargetQuest(QuestController newTarget)
    {
        if (newTarget == _targetQuest)
        {
            ClearQuestState();
        }
        else
        {
            if (_targetQuest != null)
            {
                ClearQuestState();
            }
            QuestPanel.ChangeQuestButtonState(newTarget, true);
            _targetQuest = newTarget;
            //установить таргет на свободный степ этого квеста
            RecalculateStepTarget();
        }
    }

    public void RecalculateQuestTarget()
    {
        if (_targetQuest != null || currentQuests.Count == 0) return;
        OfferQuestTarget(currentQuests.First());
    }
    
    public void RecalculateStepTarget()
    {
        if (_targetQuest == null) return;

        var availableSteps = _targetQuest.allSteps.FindAll(step => step.state == QuestState.Available);
        if (availableSteps.Count > 0)
        {
            _targetStep = availableSteps.First();
            MapScript.Instance.UpdateAllSteps(_targetQuest);
            
            var targetedType = _targetStep.needObjectType;
            localTaker.UpdateCampTriggerArea(targetedType);
            QuestPanel.ChangeStepButtonState(_targetStep, true);
        }
    }
    
    /** Обновить состояние квеста */
    public void UpdateTargetStep(QuestStep newTarget)
    {
        if (newTarget == _targetStep)
        {
            ClearStepState();
        }
        else
        {
            ClearStepState();
            if (_targetQuest != newTarget.quest)
            {
                ClearQuestState();
                QuestPanel.ChangeQuestButtonState(newTarget.quest, true);
                _targetQuest = newTarget.quest;
            }
            QuestPanel.ChangeStepButtonState(newTarget, true);
            _targetStep = newTarget;
            MapScript.Instance.UpdateAllSteps(_targetQuest);
            var targetedType = _targetStep.needObjectType;
            localTaker.UpdateCampTriggerArea(targetedType);
        }
    }

    /** Дропнуть квест */
    public void RemoveQuestTarget(QuestController target)
    {
        if (target == _targetQuest)
        {
            //убрать таргет с квеста и шагов
            ClearQuestState();
        }
    }
    
    /** Дропнуть шаг */
    public void RemoveStepTarget(QuestStep target)
    {
        if (target == _targetStep)
        {
            //убрать таргет с шага
            ClearStepState();
        }
    }
    
    /** Предложить квест как цель */
    public void OfferQuestTarget(QuestController target)
    {
        if (_targetQuest == null)
        {
            //докинуть этот квест таргетом
            _targetQuest = target;
            QuestPanelScript.Instance.ChangeQuestButtonState(target, true);
            RecalculateStepTarget();
        }
    }
    
    /** Предложить шаг как цель */
    public void OfferStepTarget(QuestStep target)
    {
        if (_targetStep == null && target.quest == _targetQuest)
        {
            //докинуть этот степ таргетом
            _targetStep = target;
            MapScript.Instance.UpdateAllSteps(_targetQuest);
            QuestPanelScript.Instance.ChangeStepButtonState(target, true);
            var targetedType = _targetStep.needObjectType;
            localTaker.UpdateCampTriggerArea(targetedType);
        }
    }
    
    private void InitTemplates()
    {
        standartDayTemplate = new List<TaskInfo>
        {
            new("Quest", "Сон", TaskType.Sleeping, false, false, 0, 420, true, false),
            new("Quest", "Приготовить завтрак", TaskType.Cooking, true, true, 420, 600, false, true, new StarQuestObjectGetter(CampObjectType.Kitchen)),
            new("Quest", "Построение", TaskType.Standing, false, false, 600, 630, true, false),
            new("Quest", "Раскопки", TaskType.Digging, false, false, 630, 870, false, false),
            new("Quest", "Приготовить обед", TaskType.Cooking, true, true, 720, 900, false, true, new StarQuestObjectGetter(CampObjectType.Kitchen)),
            new("Quest", "Раскопки", TaskType.Digging, false, false, 990, 1170, false, false),
            new("Quest", "Приготовить ужин", TaskType.Cooking, true, true, 1020, 1200, false, true, new StarQuestObjectGetter(CampObjectType.Kitchen))
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

    public void EndQuest(QuestController quest)
    {
        currentQuests.Remove(quest);
        quest.End();
    }

    [Server]
    public void CheckEndStartTasks()
    {
        var needRemoveFromCurrentTasks = new List<QuestController>();
        var needRemoveFromRemainingTasks = new List<TaskInfo>();
        
        foreach (var questController in currentQuests.Where(task => Timer.instance.gameTimer >= task.endTime))//Сначала заканчиваем таски, вдруг новый таск будет на том же месте
            needRemoveFromCurrentTasks.Add(questController);
        foreach (var questController in needRemoveFromCurrentTasks)
            currentQuests.Remove(questController);
        foreach (var questController in needRemoveFromCurrentTasks)
            questController.TimeOut();

        foreach (var task in remainingTasks.Where(task => Timer.instance.gameTimer >= task.startTime))
        {
            var questController = task.StartQuest();
            // localCommands.RpcReinitializeQuestController(
            //     task.Controller.gameObject, 
            //     task.TaskName,
            //     task.NeedStartAfterCreate
            //     );
            currentQuests.Add(questController);
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
        currentQuests.Clear();//todo веротяно с появление норм квестов, должно отлететь
    }
    
    public void SetDayDoingsTemplate()
    {
        _remainingTimeDoings = new List<AbstractTimeDoing>(StandardDayTimeDoingTemplate); //Строим текущий план действий на день
        _currentTimeDoings.Clear();
    }
}