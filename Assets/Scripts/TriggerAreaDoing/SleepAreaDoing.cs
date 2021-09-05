using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEditor;
using System.Linq;

public class SleepAreaDoing : TriggerAreaDoing
{
    [SerializeField] Vector3 playerSleepOffset = Vector3.zero;//Смещение игрока при сне, необходимо, чтобы место сна было посередине экрана
    [SerializeField] Camp camp;
    public int skipTime;
    public int maxCount = 1;
    [SerializeField] float offset = 0.3f;
    [SerializeField] float y = 0.02f;
    [SerializeField] private bool needChangeSprite = false;
    [SerializeField] private SpriteRenderer[] generalObject;
    public GameObject mainObject;
    protected virtual string[] GetLabelTexts() => new string[2] {"Лечь спать", "Встать"};
    private bool ContainsLocalPlayer =>
        new List<GameObject>(sleepers.Select(sleeper => sleeper.entityObject)).Contains(localPlayer);

    public bool HaveSpace => sleepers.Count < maxCount;

    public List<AbstractEntityInfo> sleepers = new List<AbstractEntityInfo>();

    public override bool CanInteract(GameObject interactEntity)
    {
        if (ContainsLocalPlayer) return true;
        if (camp != null)//Если зона сна - это часть палатки, то нужно чекать приват у нее//todo палатка должна быть одновременно и тем и тем, чтобы такой хуетой не заниматься
            return camp.CanInteract(interactEntity);
        return owner == null || owner == interactEntity || owner.CompareTag("Player") && !playerMetaData.privateEnable;
    }

    public override bool NeedShowLabel()
    {
        return (HaveSpace || ContainsLocalPlayer) && PlayerThere;
    }
    
    public bool AddOneEntity(AbstractEntityInfo entityInfo)
    {
        if (sleepers.Contains(entityInfo)) return false;//Возможно при коннекте

        switch (entityInfo)
        {
            case PlayerInfo playerInfo:
                playerInfo.entityObject.GetComponent<PlayerNet>().Panel = null;
                if (localCommands.isServer)
                    localPlayerNet.AddDoing(playerInfo.identity, this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
                TurnLabel(NeedShowLabel());
                break;
            case BotInfo botInfo: 
                botInfo.entityObject.transform.position = transform.position + playerSleepOffset;
                var botController = botInfo.entityObject.GetComponent<MobController>();
                botController.SetAllRenders(false);
                botInfo.entityObject.GetComponent<Collider2D>().isTrigger = true;
                break;
        }
        var spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + entityInfo.GetHeadNum);
        if (needChangeSprite)
        {
            generalObject[sleepers.Count - 1].sprite = spritesHead[3];
            UpdateSprites();
        }
        sleepers.Add(entityInfo);
        return true;
    }

    public void RemoveOneEntity(AbstractEntityInfo entityInfo)
    {
        var index = sleepers.IndexOf(entityInfo);
        if (index == -1) return;//Может возникнуть, когда спали командой
        
        switch (entityInfo)
        {
            case PlayerInfo playerInfo:
                if (!playerInfo.IsDisconnected) //Когда убираем дисконектнувшегося игрока, ничего возвращать не нужно
                    localPlayerNet.RemoveDoing(playerInfo.identity, this);//Убираем из player net текущее действие
                RefreshStateLabel();
                break;
            case BotInfo botInfo:
                botInfo.entityObject.transform.position = transform.position;//todo скорее всего с игроком делаем то же самое
                var botController = botInfo.entityObject.GetComponent<MobController>();
                botController.SetAllRenders(true);
                botInfo.entityObject.GetComponent<Collider2D>().isTrigger = false;
                break;
        }
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
        sleepers.Remove(entityInfo);
        UpdateSprites();
    }

    void UpdateSprites()
    {
        if (!needChangeSprite) return;
        for (var i = 0; i < sleepers.Count; i++)
        {
            generalObject[i].transform.position = new Vector3(
                transform.position.x - offset * ((sleepers.Count - 1) / 2f - i),
                generalObject[0].transform.position.y,
                -0.0001f * (i + 1)//чтобы были друг за другом
                );
        }
    }

    public void WakeUp()
    {
        localCommands.CmdWakeUp(mainObject, localPlayerNet.id);
    }

    /** Действия, которые необходимо выполнить только локально
     * Вызываются с сервера, после прохождения валидации на то, что можно встать
     */
    public void LocalWakeUp()
    {
        taskManager.nowSleepObject = null;
        ChangeText(GetLabelTexts()[0]);
        UpdateTextLabel();
        if (localMoving.objectsStopsThrove.Contains(gameObject))
        {
            localMoving.objectsStopsThrove.Remove(gameObject);
        }
        //pl.GetComponent<HealthBar>().multiplayer = 1;
        localPlayerInventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
        localPlayerInventoryController.inventories[0].BackInHands();//Возвращаем в руки все

        localPlayer.transform.parent = null;
        localPlayer.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
    }

    public override void DisconnectExit(string id)
    {
        if (sleepers.Contains(EntitysController.instance.GetPlayerData(id)))
        {
            localCommands.WakeUp(mainObject, id, true);
        }
    }

    public override void WasdDoing()
    {
        if (ContainsLocalPlayer)
        {
            WakeUp();
        }
    }

    public override bool Do()
    {
        if (ContainsLocalPlayer)
        {
            WakeUp();
        }
        else if (HaveSpace)
        {
            localCommands.CmdGoToSleep(mainObject);
        }
        else
        {
            return false;
        }
        return true;
    }

    /** Локальные дейтсвия перед сном, типа фриза инвентаря */
    public void LocalPrepareToSleep()
    {
        ChangeText(GetLabelTexts()[1]);
        UpdateTextLabel();
        if (!localMoving.objectsStopsThrove.Contains(gameObject))
        {
            localMoving.objectsStopsThrove.Add(gameObject);
        }
        localPlayerInventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
        localPlayer.transform.parent = transform;
        localPlayer.transform.position = transform.position + playerSleepOffset;
    }

    public void SendReadyToSkip(string id)
    {
        if (taskManager.gameTimer < skipTime)
        {
            localCommands.CmdReadyToSkipWithoutSprite(id, skipTime);
        }
    }
}
