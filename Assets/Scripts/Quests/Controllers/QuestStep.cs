using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

/** Этот объект может быть целью как квест
 * В данный момент целью может стать сам квест, либо его отдельный шаг
 */
public interface IQuestTarget
{
    public Transform GetTargetTransform();
}

public class QuestStep : NetworkBehaviourExtension, IQuestTarget
{
    public QuestState state = QuestState.Unavailable;
    public bool choosen => taskManager._targetStep == this;//Выбран ли этот степ как основной в данный момент
    
    public bool needChangeCollider = true; //Необходимо ли включать физические границы
    public CampObject currentCampObject; //Объект в лагере, на котором этот минигейм сейчас висит (например, костер)
    
    public int needCount = 3;//Требуемое колличество очков для окончания шанга
    public int currentCount;//Текущее колличество очков
    
    public QuestController quest;//Квест, шагом которого эта сущность является
    
    public bool miniGameNeedEnergy = true;//Требуется ли энергия для этого шага
    public string stepName;//Имя шага (например, обжарить лук)
    
    public bool needActive;//Нужно ли активировать миниигру сразу при активации
    public GameObject currentMiniGameObject;
    [SerializeField] protected Vector3 miniGameOffset;
    
    /** Размер и смещение размеров миниигры, нужно для ботов todo звучит ненадежно */
    public Vector2 size = Vector2.zero;
    public Vector2 offset = Vector2.zero;
    public string miniGameNamePrefabName;

    public CampObjectType needObjectType;

    public QuestStep[] needSteps;//Шаги которые необходимо выполнить перед выполнением этого шага

    public Transform GetTargetTransform()
    {
        return needObjectType != CampObjectType.Nothing ? CampObject.getByType(needObjectType).First().transform : null;
    }

     public void Start()
     {
         RequestData();
     }

     [Client]
     private void RequestData()
     {
         /** Если мы не сервер, то запрашиваем данные степа */
         if(!isServer) localCommands.CmdRequestInitQuestStep(gameObject);
     }

    public void ChangeState(QuestState newState) {
        state = newState;
        if (state == QuestState.Available)
        {
            taskManager.OfferStepTarget(this);
        }
        else
        {
            taskManager.RemoveStepTarget(this);
        }
    }

    void OnDestroy() {
        QuestStepsController.Instance.RemoveStep(this);
        taskManager.RemoveStepTarget(this);
    }

    /** Первоначальная установка всех данных */
    public void SetData(
        bool newNeedChangeCollider,
        int newNeedCount,
        int newCurrentCount,
        GameObject questObject,
        bool newMiniGameNeedEnergy,
        string newName,
        bool newNeedActive,
        string newMiniGameNamePrefab,
        int newNeedObjectType,
        QuestStep[] needStepsObjects,
        int stateNum
    )
    {
        if (quest != null) return; //Чтобы на сервере два раза не вызвать
        quest = questObject.GetComponent<QuestController>();
        needSteps = needStepsObjects;
        needChangeCollider = newNeedChangeCollider;
        needCount = newNeedCount;
        currentCount = newCurrentCount;
        miniGameNamePrefabName = newMiniGameNamePrefab;
        miniGameNeedEnergy = newMiniGameNeedEnergy;
        stepName = newName;
        needActive = newNeedActive;
        needObjectType = (CampObjectType)newNeedObjectType;//-1 если нам не нужен никакой объект, например сон

        state = (QuestState)stateNum;
        quest.AddStep(this);
        if (state == QuestState.Available)
            taskManager.OfferStepTarget(this);
        
        ChangeState((QuestState)stateNum);
        QuestStepsController.Instance.AddStep(needObjectType, this);
        if (needActive) AutoSpawnMiniGame();
    }

    [Server]
    private void AutoSpawnMiniGame()
    {
        SpawnMiniGame();//Пока что такие шаги выполняются на сервере, потому что нет еще ничего, что требовало бы другое
    }

    /** Устанавливаем на объекте */
    public void SetActive()
    {
        var campObject = CampObject.AllCampObjects.Find(obj => obj.objectType == needObjectType);
        //campObject.SetQuestStep(this, needChangeCollider, stepName);
        //Делаем кнопки там всякие в меню активными
    }
    
    public void SetCount(int count)
    {
        currentCount = count;
        CheckEnd();
    }
    
    [Server]
    void CheckEnd()
    {
        if (currentCount >= needCount)
        {
            EndMiniGame();
            localCommands.RpcQuestStepEnded(gameObject);
        }
    }
    
    public void EndMiniGame()
    {
        if (currentMiniGameObject != null)
        {
            Destroy(currentMiniGameObject);
            localPlayerInventoryController.inventories[0].BackInHands();//todo а что если мы спим например?
            currentMiniGameObject = null;
        }
    }
    
    /**
    Заспавнить миниигру
    @param spawnObject объект, с которого вызван этот метод (например, костер)
    Это необходимо для некоторых миниигр, которые берут размер (например миниигра построения должна брать размер с плаца) 
    */
    public void SpawnMiniGame()
    {
        if (miniGameNamePrefabName == null) {
            Debug.Log("У шага квеста: " + stepName + " не указано miniGameNamePrefab");
            return;
        }
        if (currentMiniGameObject != null) {
            Debug.Log("У шага квеста: " + stepName + " отменен повторно вызванный спавн миниигры");
            return;
        }
        if (state is QuestState.Unavailable or QuestState.Done) {
            Debug.Log("У шага квеста: " + stepName + " в состоянии: " + (int)state +  " отменен вызванный спавн миниигры");
            return;
        }
        if (miniGameNeedEnergy && localHealthBar.Energy < 1) return;
        currentMiniGameObject = Instantiate(Resources.Load("Minigames/" + miniGameNamePrefabName), localPlayer.transform.position + Vector3.up, Quaternion.identity, transform) as GameObject;

        var minigameComponent = currentMiniGameObject.GetComponent<StandartMinigame>();
        if (minigameComponent != null)
            minigameComponent.Init();
        if (!needActive)//Если это полноценная миниигра, а не самоактивирующаяся
        {
            localCommands.CmdDestroyObjectInHands(localPlayer);
            localCommands.CmdSpawnObjectInHands(localPlayer, new string[] { "Doing" });
        }

        if (size != Vector2.zero && needChangeCollider)
         {
             var c = currentMiniGameObject.GetComponent<BoxCollider2D>();
             c.size = size;
             c.offset = offset;
         }

        var minigame = currentMiniGameObject.GetComponent<Minigame>();
        minigame.step = this;
        minigame.ChangeName(stepName);
    }
    
    /**Меняет состояния выбран/доступен**/
    public void ChangeTargetState(bool newState)
    {
        // _taskController.GetComponent<MapScript>().SetState(taskPoint.mapCircleObject, newState ? StepState.Chosen : StepState.Enable);//todo
        // ownButton.ChangeTarget(newState);
    }

    public void MiniGameFinish()
    {
        localCommands.CmdMiniGameSet(gameObject);
    }
    
    /** Досрочное завершение миниигры, например, из-за движения */
    public void PreEndGame()
    {
        EndMiniGame();
    }
}
