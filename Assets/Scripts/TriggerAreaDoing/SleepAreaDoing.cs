using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEditor;

public class SleepAreaDoing : TriggerAreaDoing
{

    public int skipTime;
    public int maxCount = 1;
    [SerializeField] float offset = 0.3f;
    [SerializeField] float y = 0.02f;
    [SerializeField] private bool needChangeSprite = false;
    [SerializeField] private SpriteRenderer[] generalObject;
    public GameObject mainObject;
    Debaf currentZharaDebaf;
    Debaf currentChillBaf;
    Debaf currentSleepBaf;
    readonly string[] labelTexts = new string[2] {"Лечь спать", "Встать"};

    public List<string> sleepers = new List<string>();
    TaskManager taskManagerRef;
    TaskManager TaskManagerRef {
        get
        {
            if (taskManagerRef == null)
                taskManagerRef = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
            return taskManagerRef;
        }
    }
    PlayerNet playNet;
    public PlayerNet PlayNet
    {
        get
        {
            if (playNet == null)
                playNet = Player.GetComponent<PlayerNet>();
            return playNet;
        }
        set => playNet = value;
    }
    GameObject player;
    public GameObject Player
    {
        get
        {
            if (player == null)
                player = GameObject.Find("LocalPlayer");
            return player;
        }
        set => player = value;
    }
    Moving Mov => PlayNet._moving;
    Commands Cmd => PlayNet.cmd;
    PlayerInventoryController InventoryController => PlayNet.inventoryController;

    DebafsController debafsControllerRef;
    DebafsController DebafsControllerRef
    {
        get
        {
            if (debafsControllerRef == null)
                debafsControllerRef = FindObjectOfType<DebafsController>();
            return debafsControllerRef;
        }
    }

    public override bool NeedShowLabel()
    {
        return (sleepers.Count < maxCount || sleepers.Contains(PlayNet.id)) && PlayerThere;
    }

    public void AddOne(string newSleeper, Sprite sprite = null)
    {
        PlayNet.AddDoing(newSleeper, this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
        sleepers.Add(newSleeper);
        TurnLabel(NeedShowLabel());
        if (needChangeSprite)
        {
            generalObject[sleepers.Count - 1].sprite = sprite;
            UpdateSprites();
        }
    }

    void UpdateSprites()
    {
        for (var i = 0; i < sleepers.Count; i++)
        {
            generalObject[i].transform.position = new Vector3(
                transform.position.x - offset * ((sleepers.Count - 1) / 2f - i),
                generalObject[0].transform.position.y,
                -0.0001f * (i + 1)//чтобы были друг за другом
                );
        }
    }

    public void RemoveOne(string newSleeper, bool needReturnItems)
    {
        var index = sleepers.IndexOf(newSleeper);
        if (index == -1) return;//Может возникнуть, когда спали командой
        if (needReturnItems) //Когда убираем дисконектнувшегося игрока, ничего возвращать не нужно
            PlayNet.RemoveDoing(newSleeper, this);//Убираем из player net текущее действие
        if (needChangeSprite)
        {
            for (var i = index; i < sleepers.Count; i++)
            {
                if (i == sleepers.Count - 1)
                    generalObject[i].sprite = null;
                else
                    generalObject[i].sprite = generalObject[i + 1].sprite;
            }
        }
        sleepers.Remove(newSleeper);
        UpdateSprites();
        TurnLabel(NeedShowLabel());
    }

    public void VakeUp()
    {
        if (currentZharaDebaf != null)
            currentZharaDebaf.Off();
        if (currentChillBaf != null)
            currentChillBaf.Off();
        if (currentSleepBaf != null)
            currentSleepBaf.Off();
        TaskManagerRef.nowSleepObject = null;
        ChangeText(labelTexts[0]);
        //sleepers.Remove(player);
        PlayNet.CmdWakeUp();
        if (Mov.objectsStopsThrove.Contains(gameObject))
        {
            Mov.objectsStopsThrove.Remove(gameObject);
        }
        //pl.GetComponent<HealthBar>().multiplayer = 1;
        InventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
        InventoryController.inventories[0].BackInHands();//Возвращаем в руки все

        Player.transform.parent = null;
        Player.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.y / 100);
        Cmd.CmdWakeUp(mainObject, PlayNet.id, true);
    }

    public override void DisconectExit(string id)
    {
        if (sleepers.Contains(id))
        {
            Cmd.WakeUp(mainObject, id, false, true);
        }
    }

    public override void WasdDoing(GameObject player)
    {
        if (sleepers.Contains(PlayNet.id))
        {
            VakeUp();
            UpdateTextLabel();
        }
    }

    public override bool Do(GameObject sleepPlayer)
    {
        //TurnLabel(false);
        if (PlayNet == null)
        {
            Player = sleepPlayer;
            PlayNet = Player.GetComponent<PlayerNet>();
        }

        if (sleepers.Contains(PlayNet.id))
        {
            VakeUp();
        }
        else if (sleepers.Count < maxCount)
        {
            ChangeText(labelTexts[1]);
            //sleepers.Add(player);

            //player.GetComponent<HealthBar>().multiplayer = 10;
            playNet.CmdUnstuck();
            Mov.plNet.cmd.CmdDestroyObjectInHands(Mov.gameObject); //Убираем из рук все вещи
            if (!Mov.objectsStopsThrove.Contains(gameObject))
            {
                Mov.objectsStopsThrove.Add(gameObject);
            }
            InventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
            sleepPlayer.transform.parent = transform;
            sleepPlayer.transform.position = transform.position;
            if (TaskManagerRef.gameTimer < skipTime)
            {
                Cmd.CmdReadyToSkip(PlayNet.id, skipTime, mainObject, playNet._moving.headNum);
                currentSleepBaf = DebafsControllerRef.AddDebaf(3, false);//кидаем сон
            }
            else
            {
                Cmd.CmdReadyToSkip(PlayNet.id, -1, mainObject, playNet._moving.headNum);
                TaskManagerRef.nowSleepObject = this;
                currentZharaDebaf = DebafsControllerRef.AddDebaf(0, false);//кидаем жару
                currentChillBaf = DebafsControllerRef.AddDebaf(2, false);//кидаем чил
            }
        }
        else
        {
            return false;
        }
        UpdateTextLabel();
        return true;
    }

    public void SendReadyToSkip(string id)
    {
        if (TaskManagerRef.gameTimer < skipTime)
        {
            Cmd.CmdReadyToSkipWithoutSprite(id, skipTime);
            if (currentZharaDebaf != null)
                currentZharaDebaf.Off();
            if (currentChillBaf != null)
                currentChillBaf.Off();
            currentSleepBaf = DebafsControllerRef.AddDebaf(3, false);//кидаем сон
        }
    }
}
