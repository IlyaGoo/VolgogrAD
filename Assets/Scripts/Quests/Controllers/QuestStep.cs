using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestStep : NetworkBehaviourExtension
{
    public QuestState state = QuestState.Available;
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
    public string miniGameNamePrefab = null;

    public CampObjectType needObjectType;

    public QuestStep[] needSteps;//Шаги которые необходимо выполнить перед выполнением этого шага

     public void Start()
    {
        localCommands.CmdRequestInitQuestController(gameObject);
    }

    public void ChangeState(QuestState newState) {
        state = newState;
        QuestStepsController.instance.ResortSteps(needObjectType);
    }

    void OnDestroy() {
        QuestStepsController.instance.RemoveStep(needObjectType, this);
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
        string newMiniGameNamePrefab, //todo
        int newNeedObjectType,
        GameObject[] needStepsObjects
    )
    {
        needSteps = needStepsObjects.Select(obj => obj.GetComponent<QuestStep>()).ToArray();
        needChangeCollider = newNeedChangeCollider;
        needCount = newNeedCount;
        currentCount = newCurrentCount;
        miniGameNamePrefab = newMiniGameNamePrefab;
        quest = questObject.GetComponent<QuestController>();
        miniGameNeedEnergy = newMiniGameNeedEnergy;
        stepName = newName;
        needActive = newNeedActive;
        needObjectType = (CampObjectType)newNeedObjectType;//-1 если нам не нужен никакой объект, например сон
        if (needActive && isServer) SpawnMiniGame();//Пока что такие шаги выполняются на сервере, потому что нет еще ничего, что требовало бы другое
        QuestStepsController.instance.AddStep(needObjectType, this);
    }

    /** Устанавливаем на объекте */
    public void SetActive()
    {
        var campObject = CampObject.allCampObjects.Find(obj => obj.objectType == needObjectType);
        //campObject.SetQuestStep(this, needChangeCollider, stepName);
        //Делаем кнопки там всякие в меню активными
    }
    
    public void SetCount(int count)
    {
        currentCount = count;
        CheckEnd();
    }
    
    void CheckEnd()//todo
    {
        if (currentCount == needCount && isServer)
        {
            EndGame();
            // _taskController.MinigameEnded(number, taskPoint);
        }
    }
    
    public void EndGame()
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
    public void SpawnMiniGame(GameObject spawnObject = null)
    {
        if (miniGameNamePrefab == null) {
            Debug.Log("У шага квеста: " + stepName + " не указано miniGameNamePrefab");
            return;
        }
        if (currentMiniGameObject != null) {
            Debug.Log("У шага квеста: " + stepName + " отменен повторно вызванный спавн миниигры");
            return;
        }
        if (state == QuestState.Unavailable || state == QuestState.Done) {
            Debug.Log("У шага квеста: " + stepName + " в состоянии: " + (int)state +  " отменен вызванный спавн миниигры");
            return;
        }
        if (miniGameNeedEnergy && localHealthBar.Energy < 1) return;
        currentMiniGameObject = Instantiate(Resources.Load("Minigames/" + miniGameNamePrefab), transform.position + miniGameOffset, Quaternion.identity, transform) as GameObject;

        StandartMinigame minigameComponent = currentMiniGameObject.GetComponent<StandartMinigame>();
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
        //reinit было
        EndGame();
    }
}
