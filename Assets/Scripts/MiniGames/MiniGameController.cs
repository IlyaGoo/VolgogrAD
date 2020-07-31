using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class MiniGameController : MonoBehaviour
{

    public int needCount = 3;
    public int currentCount = 0;
    public TaskControllerScript _taskController;
    [SerializeField] Vector3 miniGameOffset = new Vector3();

    [SerializeField] private GameObject minigamePrafab;
    public GameObject currentMiniGameObject;
    public int number;
    public bool miniGameNeedEnergy = true;
    private int pointNumber;
    public int startNumber;
    private string currentName;
    public ButtonInfo ownButton;
    public bool ready = false;
    public Point ownPoint;
    [SerializeField] bool needActive = false;

    public Vector2 size = Vector2.zero;
    public Vector2 offset = Vector2.zero;

    public void MinigameStart(TaskControllerScript taskController, int num, int need, string name, int pn, ButtonInfo newButton, int startNum)
    {
        if (currentMiniGameObject != null || needCount == currentCount) return;
        ready = true;
        _taskController = taskController;
        number = num;
        pointNumber = pn;
        currentCount = 0;
        needCount = need;
        ownButton = newButton;
        startNumber = startNum;
        currentName = name;
        TriggerAreaDoing t = GetComponent<TriggerAreaDoing>();
        if (t != null)
            t.ChangeText(name);
        GetComponent<BoxCollider2D>().enabled = true;

        if (needActive) SpawnMiniGame();
    }

    public void ReinitializeTrigger()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        //textObject.transform.parent.gameObject.SetActive(true);
    }

    public void ChangeData(TaskControllerScript taskController, int num, int need, string name, int pn, int startNum)
    {
        EndGame();
        var menuScript = taskController.menuScript;
        var targetMenu = taskController.menuScript.targetMenu;
        if (targetMenu != null && targetMenu.targetButton != null && targetMenu.targetButton.numberCostil != pn) menuScript.ChangeTarget(targetMenu.targetButton);
        //ownButton.ownMenu
        MinigameStart(taskController, num, need, name, pn, ownButton, startNum);
        if (ownButton != null)
        {
            ownButton.text = name;
            ownButton.numberCostil = pn;
            ownButton.ChangePoint(pn);
            ownButton.buttonObject.GetComponent<TextMeshProUGUI>().SetText(name);
        }

        taskController.menuScript.ChangeTarget(ownButton);

        if (needActive) SpawnMiniGame();
    }

    public void SpawnMiniGame()
    {
        if (currentMiniGameObject != null || miniGameNeedEnergy && _taskController._mobManager.playNet.healthBar.Energy < 1) return;
        currentMiniGameObject = Instantiate(minigamePrafab, transform.position + miniGameOffset, Quaternion.identity, transform);

        StandartMinigame minigameComponent = currentMiniGameObject.GetComponent<StandartMinigame>();
        if (minigameComponent != null)
            minigameComponent.Init(_taskController._mobManager.playNet.healthBar);
        if (!needActive)
        {
            _taskController._mobManager.playNet.cmd.CmdDestroyObjectInHands(_taskController._mobManager.playNet.gameObject);
            _taskController._mobManager.playNet.cmd.CmdSpawnObjectInHands(_taskController._mobManager.playNet.gameObject, new string[] { "Doing" });
        }

        if (size != Vector2.zero)
        {
            var c = currentMiniGameObject.GetComponent<BoxCollider2D>();
            c.size = size;
            c.offset = offset;
        }

        var minigame = currentMiniGameObject.GetComponent<Minigame>();
        minigame._controller = this;
        minigame.ChageName(currentName);

        if (!needActive)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            //textObject.transform.parent.gameObject.SetActive(false);
        }
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
            gameObject.SetActive(false);
            _taskController.MinigameEnded(number, pointNumber);
        }
    }

    public void EndGame()
    {
        if (currentMiniGameObject != null)
        {
            Destroy(currentMiniGameObject);
            _taskController._mobManager.playNet.inventoryController.inventories[0].BackInHands();
            currentMiniGameObject = null;
        }

        ready = false;
        currentCount = 0;
        GetComponent<BoxCollider2D>().enabled = false;
        //textObject.transform.parent.gameObject.SetActive(false);

    }

    public void PreEndGame()
    {
        EndGame();
        ReinitializeTrigger();
    }
}

public class Minigame : MonoBehaviour
{
    public MiniGameController _controller;
    public bool startSpawn;
    [SerializeField] TextMeshPro _textMesh;

    public void ChageName(string newName)
    {
        if (_textMesh != null)
            _textMesh.text = newName;
    }

    public void Finish()
    {
        _controller._taskController._mobManager.cmd.CmdMiniGamePlus(_controller._taskController.gameObject , _controller.number);
    }
}