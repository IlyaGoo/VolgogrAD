using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SleepAreaDoing : TriggerAreaDoing
{
    [SerializeField] Vector3 playerSleepOffset = Vector3.zero;//Смещение игрока при сне, необходимо, чтобы место сна было посередине экрана
    [SerializeField] Camp camp;//todo можно не указывать ручками а получать с парента
    public int skipTime;
    [SerializeField] private int maxEntityCount = 1;
    
    [SerializeField] private float headOffset = 0.3f;//Расстояние между головами
    [SerializeField] private Vector3 zeroHeadPosition = Vector3.zero;
    [SerializeField] private bool needChangeSprite;//Нужно ли спавнить головы
    [SerializeField] private GameObject headPrefab;
    
    private readonly List<SleeperHead> _sleeperHeads = new List<SleeperHead>();
    public GameObject mainObject;
    protected virtual string[] GetLabelTexts() => new string[2] {"Лечь спать", "Встать"};
    private bool ContainsLocalPlayer => sleepers.Exists(sleeper => sleeper.Identity == localPlayerId);

    public bool HaveSpace => sleepers.Count < maxEntityCount;

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
                playerInfo.EntityObject.GetComponent<PlayerNet>().Panel = null;
                if (localCommands.isServer)
                    localPlayerNet.AddDoing(playerInfo.Identity, this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
                TurnLabel(NeedShowLabel());
                break;
            case BotInfo botInfo: 
                botInfo.EntityObject.transform.position = transform.position + playerSleepOffset;
                var botController = botInfo.EntityObject.GetComponent<MobController>();
                botController.SetAllRenders(false);
                botInfo.EntityObject.GetComponent<Collider2D>().isTrigger = true;
                break;
        }
        if (needChangeSprite)
        {
            var newHead = Instantiate(headPrefab, transform.position, transform.rotation, transform);
            var newSleeperHeadComponent = newHead.GetComponent<SleeperHead>();
            newSleeperHeadComponent.Init(entityInfo);
            _sleeperHeads.Add(newSleeperHeadComponent);
            MoveHeadSprites();
        }
        sleepers.Add(entityInfo);
        return true;
    }

    public void RemoveOneEntity(AbstractEntityInfo entityInfo)
    {
        var index = sleepers.IndexOf(entityInfo);
        if (index == -1) return;//Может возникнуть, когда спали командой todo а я точно не хуйню сморозил?
        
        switch (entityInfo)
        {
            case PlayerInfo playerInfo:
                if (!playerInfo.IsDisconnected) //Когда убираем дисконектнувшегося игрока, ничего возвращать не нужно
                    localPlayerNet.RemoveDoing(playerInfo.Identity, this);//Убираем из player net текущее действие
                RefreshStateLabel();
                break;
            case BotInfo botInfo:
                botInfo.EntityObject.transform.position = transform.position;//todo скорее всего с игроком делаем то же самое
                var botController = botInfo.EntityObject.GetComponent<MobController>();
                botController.SetAllRenders(true);
                botInfo.EntityObject.GetComponent<Collider2D>().isTrigger = false;
                break;
        }

        sleepers.Remove(entityInfo);
        if (needChangeSprite) RemoveHeadSprite(entityInfo);
    }

    private void RemoveHeadSprite(AbstractEntityInfo entityInfo)
    {
        var removingHead = _sleeperHeads.Find(head => head.OwnerInfo == entityInfo);
        _sleeperHeads.Remove(removingHead);
        removingHead.SleeperWakeUp();
        MoveHeadSprites();
    }

    /** Сдвинуть головы существ в спальнике */
    private void MoveHeadSprites()
    {
        if (!needChangeSprite) return;
        var firstHeadPosition = transform.position + zeroHeadPosition + new Vector3(-headOffset * (_sleeperHeads.Count - 1) / 2f, 0, -0.0001f);
        for (var i = 0; i < _sleeperHeads.Count; i++)
        {
            _sleeperHeads[i].transform.position = firstHeadPosition + new Vector3(headOffset * i, 0, -0.0001f * i);
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
        Timer.instance.nowSleepObject = null;
        ChangeText(GetLabelTexts()[0]);
        UpdateTextLabel();
        if (localMoving.objectsStopsThrove.Contains(gameObject))
        {
            localMoving.objectsStopsThrove.Remove(gameObject);
        }
        localPlayerInventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
        localPlayerInventoryController.inventories[0].BackInHands();//Возвращаем в руки все

        localPlayer.transform.parent = null;
        localPlayer.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
    }

    public override void DisconnectExit(string id)
    {
        if (sleepers.Exists(entityInfo => entityInfo.Identity == id))
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

    /** Локальные действия перед сном, типа фриза инвентаря */
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
        if (Timer.instance.gameTimer < skipTime)
        {
            localCommands.CmdReadyToSkipWithoutSprite(id, skipTime);
        }
    }
}
