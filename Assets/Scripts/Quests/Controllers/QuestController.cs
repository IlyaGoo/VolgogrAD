using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/**
 * Контроллер какого-либо квеста
 * Спавнится вместе с объектом
 */
public class QuestController : NetworkBehaviourExtension, IQuestTarget
{
    public bool started;//Начат ли квест, чтобы случайно не прилетело сразу две команды начать его
    public List<QuestStartAreaDoing> starters = new();//Триггеры, стартующие квест

    public QuestInfo _сurrentPlan;
    public string currentQuestName;
    public TaskType taskType;//todo
    public QuestState state = QuestState.Unavailable;
    public bool needMap;

    /** Указывать ли сейчас стрелочкой на этот квест */
    public bool isTarget;

    public bool simple;
    public List<QuestStep> allSteps = new();
    private QuestButton _linqQuestButton;
    public int endTime;

    /** Получить имя XML-файла текущего квеста */
    public string GetCurrentPlanName()
    {
        return _сurrentPlan.fileName;
    }

    public void AddStep(QuestStep step) {
        allSteps.Add(step);
        if (!simple && step.state == QuestState.Available)
            _linqQuestButton.AddButton(step);
    }

    /** Инициализировать случайный план по типу квеста */
    [Server]
    private void SelectQuestByType()
    {
        var availableQuestInfos = taskType switch
        {
            TaskType.Cooking => new[] { QuestsDataStore.Tushnyak },
            TaskType.Digging => new[] { QuestsDataStore.Rascop },
            TaskType.Standing => new[] { QuestsDataStore.Postr },
            TaskType.Sleeping => new[] { QuestsDataStore.Sleep },
            _ => throw new NotImplementedException("В QuestController не указаны plans для такого типа квестов")
        };
        _сurrentPlan = availableQuestInfos[Random.Range(0, availableQuestInfos.Length)];
        simple = _сurrentPlan.simple;
    }

    /** Инициализация квеста на сервере */
    [Server]
    public void ServerInit(string taskName, TaskType type, int newEndTime, bool newNeedMap)
    {
        InitMainParameters(taskName, type, newEndTime, newNeedMap);
        SelectQuestByType();
    }

    /** Инициализировать основные параметры квеста */
    private void InitMainParameters(string taskName, TaskType type, int newEndTime, bool newNeedMap)
    {
        needMap = newNeedMap;
        state = QuestState.Available;
        taskType = type;
        currentQuestName = taskName;
        _linqQuestButton = QuestPanelScript.Instance.AddQuestButton(this, newNeedMap);
        endTime = newEndTime;
        taskManager.OfferQuestTarget(this);
    }

    /** Инициализировать квест на клиенте после спавна */
    [Client]
    public void ClientInit(string taskName, TaskType type, string planName, int newEndTime, bool newNeedMap) {
        if (isServer) return;
        InitMainParameters(taskName, type, newEndTime, newNeedMap);
        _сurrentPlan = QuestsDataStore.GetByName(planName);
        simple = _сurrentPlan.simple;
    }   

    public void Start()
    {
        /** Если мы не сервер, мы запрашиваем информацию о квесте у сервера сразу после спавна объекта */
        RequestData();
    }

    [Client]
    private void RequestData()
    {
        localCommands.CmdRequestInitQuestController(gameObject);
    }

    /** Если период доступности квеста вышел */
    [Server]
    public void TimeOut(){
        End();
    }
    
    /** Вызывает когда квест выполнен или иссякло время */
    public void End() {
        QuestPanelScript.Instance.RemoveQuestButton(_linqQuestButton);
        taskManager.RemoveQuestTarget(this);
        taskManager.RecalculateQuestTarget();
        MapScript.Instance.CloseMap(this);
        if (!isServer) return;
        foreach (var starter in starters) {
            Destroy(starter);
        }
        Destroy(gameObject);
    }

    public void EndStep(QuestStep step)
    {
        step.ChangeState(QuestState.Done);
        _linqQuestButton.RemoveButton(step);
        UpdateStepsState();
        QuestStepsController.Instance.RemoveStep(step);
        if (allSteps.TrueForAll(questStep => questStep.state == QuestState.Done))
        {
            taskManager.EndQuest(this);
        }
    }

    public void UpdateStepsState() {
        allSteps.ForEach(step =>
            {
                if (step.state != QuestState.Done && step.state != QuestState.Available && (!step.needSteps.Any() || step.needSteps.All(needStep => needStep.state == QuestState.Done)))
                {
                    step.ChangeState(QuestState.Available);
                    _linqQuestButton.AddButton(step);
                }
            }
        );
    }

    /** Старт квеста */
    public void StartQuest()
    {
        foreach (var questStartAreaDoing in starters)
        {
            questStartAreaDoing.gameObject.SetActive(false);
        }
    }

    /**
     * Выбор плана квеста и генерация шагов
     * Вызывается только на сервере
     */
    [Server]
    public void ChooseAndSpawnSteps()
    {
        if (started)//Проверка на всякий случай, вдруг как-то удастся два раза вызвать команду инициализации
        {
            Debug.Log("Попытка инициализировать QuestController, который уже инициализирован");
            return;
        }
        started = true;
        var stepsObjects = new List<GameObject>();
        foreach (var step in _сurrentPlan.allSteps)
        {
            var transform1 = transform;
            var stepObject = Instantiate(Resources.Load("Quests/QuestStep"), transform1.position, transform1.rotation, transform1) as GameObject;
            stepsObjects.Add(stepObject);

            var stepComponent = stepObject.GetComponent<QuestStep>();

            var previousSteps = step.previousStepsNumbers.Select(number => stepsObjects[number].GetComponent<QuestStep>()).ToArray();

            var needState =
                !previousSteps.Any() ||
                previousSteps.All(needStep => needStep.state == QuestState.Done)
                    ? QuestState.Available
                    : QuestState.Unavailable;

            stepComponent.SetData(//Устанавливаем данные, чтобы клиенту было что стянуть (а также спавним, если требуется)
                step.needChangeCollider,
                step.needCount,
                0,
                gameObject,
                step.miniGameNeedEnergy,
                step.stepName,
                step.needActive,
                step.miniGameNamePrefab,
                step.needObjectType,
                previousSteps,
                (int)needState
                );
            NetworkServer.Spawn(stepObject);
        }
    }

    public Transform GetTargetTransform()
    {
        return starters.Count == 0 ? null : starters[0].transform;//todo брать ближний
    }
}
