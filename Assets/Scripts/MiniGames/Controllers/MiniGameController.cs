using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class MiniGameController : MonoBehaviourExtension
{
    [SerializeField] bool needChangeCollider = true;
    
    public ButtonInfo ownButton;
    public Point ownPoint;

    public int needCount = 3;
    public int currentCount = 0;
    public bool ready = false;
    public TaskControllerScript _taskController;

    public int number;
    public TaskPoint taskPoint;

    public bool miniGameNeedEnergy = true;
    protected string currentName;
    [SerializeField] protected bool needActive = false;

    [SerializeField] protected GameObject minigamePrafab;
    public GameObject currentMiniGameObject;
    [SerializeField] protected Vector3 miniGameOffset = new Vector3();

    public Vector2 size = Vector2.zero;
    public Vector2 offset = Vector2.zero;

    public void MinigameStart(TaskControllerScript taskController, int num, int need, string name, TaskPoint newTaskPoint)
    {
        if (currentMiniGameObject != null || needCount == currentCount) return;
        ready = true;
        _taskController = taskController;
        number = num;
        taskPoint = newTaskPoint;
        currentCount = 0;
        needCount = need;
        currentName = name;
        TriggerAreaDoing t = GetComponent<TriggerAreaDoing>();
        if (t != null)
            t.ChangeText(name);
        if (needChangeCollider)
            GetComponent<BoxCollider2D>().enabled = true;

        if (needActive) SpawnMiniGame();
    }

    public void ReinitializeTrigger()
    {
        if (needChangeCollider)
            GetComponent<BoxCollider2D>().enabled = true;
        //textObject.transform.parent.gameObject.SetActive(true);
    }

    public void AddOne()//больше не используется
    {
        currentCount++;
        CheckEnd();
    }

    public void SetCount(int count)
    {
        currentCount = count;
        CheckEnd();
    }

    void CheckEnd()
    {
        if (currentCount == needCount)
        {
            EndGame();
            //_taskController.ended(number);
            if (_taskController != null)
            {
                gameObject.SetActive(false);
                _taskController.MinigameEnded(number, taskPoint);
            }
        }
    }

    public void EndGame()
    {
        if (currentMiniGameObject != null)
        {
            Destroy(currentMiniGameObject);
            localPlayerInventoryController.inventories[0].BackInHands();
            currentMiniGameObject = null;
        }

        //ready = false;
        currentCount = 0;
        if (needChangeCollider)
            GetComponent<BoxCollider2D>().enabled = false;
        //textObject.transform.parent.gameObject.SetActive(false);

    }

    public void SpawnMiniGame()
    {
        if (currentMiniGameObject != null || miniGameNeedEnergy && localHealthBar.Energy < 1) return;
        currentMiniGameObject = Instantiate(minigamePrafab, transform.position + miniGameOffset, Quaternion.identity, transform);

        StandartMinigame minigameComponent = currentMiniGameObject.GetComponent<StandartMinigame>();
        if (minigameComponent != null)
            minigameComponent.Init();
        if (!needActive)
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
        minigame._controller = this;
        minigame.ChangeName(currentName);

        if (!needActive && needChangeCollider)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            //textObject.transform.parent.gameObject.SetActive(false);
        }
    }

    /**Меняет состояния выбран/доступен**/
    public void ChangeTargetState(bool newState)
    {
        _taskController.GetComponent<MapScript>().SetState(taskPoint.mapCircleObject, newState ? CircleState.Choosen : CircleState.Enable);//todo
        ownButton.ChangeTarget(newState);
    }

    public void ChangeData(TaskControllerScript taskController, int need, TaskPoint newTaskPoint)
    {
        EndGame();
        //ownButton.ownMenu
        MinigameStart(taskController,  newTaskPoint.pointData.minigameControllerNum, need, newTaskPoint.pointData.name, newTaskPoint);
        this.ownButton = ownButton;
        if (ownButton != null)
        {
            ownButton.text = newTaskPoint.pointData.name;
            ownButton.ChangePoint(this);
            ownButton.buttonObject.GetComponent<TextMeshProUGUI>().SetText(newTaskPoint.pointData.name);
        }
        if (needActive) SpawnMiniGame();
    }

    public void MiniGameFinish()
    {
        if (_taskController != null)
            localCommands.CmdMiniGameSet(_taskController.gameObject, number, true);
        else
            localCommands.CmdMiniGameSet(gameObject, number, false);
    }

    public void PreEndGame()
    {
        EndGame();
        ReinitializeTrigger();
    }
}

public class Minigame : MonoBehaviourExtension
{
    public MiniGameController _controller;
    public bool startSpawn;
    [SerializeField] TextMeshPro _textMesh;

    public void ChangeName(string newName)
    {
        if (_textMesh != null)
            _textMesh.text = newName;
    }

    public void Finish()
    {
        _controller.MiniGameFinish();
    }
}