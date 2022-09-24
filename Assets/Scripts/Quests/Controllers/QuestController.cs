using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/** Контроллер какого-либо квеста
Спавнится вместе с объектом
 */
public class QuestController : NetworkBehaviourExtension
{
    public bool started = false;//Начат ли квест, чтобы случайно не прилетело сразу две команды начать его
    private QuestInfo[] _plans; //массив последовательностей действий
    public List<QuestStartAreaDoing> starters = new List<QuestStartAreaDoing>();//Триггеры, стартующие квест

    private readonly List<QuestStep> _steps = new List<QuestStep>();
    
    private QuestInfo CurrentPlan => _currentPlanNum == -1 ? null : _plans[_currentPlanNum];
    public int _currentPlanNum = -1;
    
    [SerializeField] private GameObject stepControllerPrefab;
    public string currentQuestName;
    public TaskType taskType;//todo
    public QuestState state = QuestState.Unavailable;

    /** Указывать ли сейчас стрелочкой на этот квест */
    public bool isTarget;

    /** Инициализировать квест после спавна */
    public void Init(string taskName, TaskType type, int planNum = 0){
        if (isServer) {
            _currentPlanNum = Random.Range(0, _plans.Length);//На сервере сразу выбираем план квеста
        } else {
            _currentPlanNum = planNum;
        }

        state = QuestState.Available;

        switch (taskType) //Нужно присвоить текущее граф таска, типа что за чем следует
        {
            case TaskType.Cooking:
                _plans = new[] { QuestsDataStore.Tushnyak };
                break;
            case TaskType.Digging:
                _plans = new[] { QuestsDataStore.Rascop };
                break;
            case TaskType.Standing:
                _plans = new[] { QuestsDataStore.Postr };
                break;
            case TaskType.Sleeping:
                _plans = new[] { QuestsDataStore.Postr };
                break;
            default:
                throw new NotImplementedException("В QuestController не указаны plans для такого типа квестов");
        }
        taskType = type;
        currentQuestName = taskName;
    }   

    public void Start()
    {
        /** Если мы не сервер, мы запрашиваем информацию о квесте у сервера сразу после спавна объекта */
        if (!isServer) {
            localCommands.CmdRequestInitQuestController(gameObject);
        }
    }

    /** Если период доступности квеста вышел */
    public void TimeOut(){
        if (!isServer) return;
        End();
    }

    public void End() {
        if (!isServer) return;
        foreach (var starter in starters) {
            Destroy(starter);
        }
        Destroy(gameObject);
    }

    /**
     * Выбор плана квеста и генерация шагов
     * Вызывается только на сервере
     */
    public void ChooseAndSpawnSteps()//server
    {
        started = true;
        var stepsObjects = new List<GameObject>();
        foreach (var step in CurrentPlan.allSteps)
        {
            var stepObject = Instantiate(stepControllerPrefab, transform.position, transform.rotation, transform);
            stepsObjects.Add(stepObject);

            var stepComponent = stepObject.GetComponent<QuestStep>();
            stepComponent.SetData(//Устанавливаем данные, чтобы клиенту было что стянуть
                step.needChangeCollider,
                step.needCount,
                0,
                step.questObject,
                step.miniGameNeedEnergy,
                step.stepName,
                step.needActive,
                step.miniGameNamePrefab,
                step.needObjectType,
                step.previousStepsNumbers.Select(number => stepsObjects[number]).ToArray()
                );
            NetworkServer.Spawn(stepObject);
        }
    }
}
