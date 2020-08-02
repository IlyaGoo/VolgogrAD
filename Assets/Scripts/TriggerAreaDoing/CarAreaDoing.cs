using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAreaDoing : TriggerAreaDoing, IListener, ICanBeOwn
{
    [SerializeField] AudioSource motor;
    private string driver;
    GameObject driverObject;
    public List<string> passagers = new List<string>();
    public int MaxCount = 10;
    bool canDrive = false;
    public CarMoving carComponent;
    public CarSyn synComponent;
    bool PlayerInCar = false;
    public GameObject owner = null;
    public GameObject Owner { get => owner; set => owner = value; }

    protected override Transform GetPanelParent() => transform.parent;//Иначе при повороте надпись будет крутиться

    public string Driver 
    {
        get => driver;
        set 
        { 
            driver = value;
            var data = taskManager.GetPlayerData(value);
            driverObject = data == null ? null : taskManager.GetPlayerData(value).playerObject;
        } 
    }

    public override bool NeedShowLabel()
    {
        return !PlayerInCar && PlayerThere;
    }

    public bool CanInteract(GameObject mayBeOwner)
    {
        return owner == null || owner == mayBeOwner || (owner.CompareTag("Player") && !playerMetaData.privateItems);
    }

    public void AddOne(string newPassagerId, bool isDriver, GameObject newPassager)
    {
        if (passagers.Count == 0)
        {
            motor.Play();
        }
        localPlayer.GetComponent<PlayerNet>().AddDoing(newPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
        newPassager.GetComponent<CircleCollider2D>().enabled = false;
        if (isDriver)
            Driver = newPassagerId;
        carComponent.passagers.Add(newPassager);
        passagers.Add(newPassagerId);
        TurnLabel(NeedShowLabel());

        if (localPlayer == newPassager)
            SitIn(isDriver, newPassager);

        newPassager.transform.parent = gameObject.transform.parent;
        newPassager.transform.position = gameObject.transform.position + carComponent.passagersOffset;
    }

    public void RemoveOne(string removingPassagerId, bool isDriver, GameObject removingPassager)
    {
        if (removingPassager == null)
            carComponent.ClearMissingsPassagers();
        else  //Когда убираем дисконектнувшегося игрока, ничего возвращать не нужно
        {
            playerNet.RemoveDoing(removingPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Убираем из player net текущее действие
            removingPassager.GetComponent<CircleCollider2D>().enabled = true;
            removingPassager.transform.parent = null;
        }
        
        passagers.Remove(removingPassagerId);
        if (passagers.Count == 0)
        {
            motor.Stop();
        }
        carComponent.passagers.Remove(removingPassager);
        
        TurnLabel(NeedShowLabel());
        if (isDriver)
            Driver = null;

        if (localPlayer == removingPassager)
            SitOut(isDriver, removingPassager);
    }

    public void SitOut(bool isDriver, GameObject player)
    {
        if (isDriver)
        {
            carComponent.canMove = canDrive = false;
            synComponent.isDriver = false;
            var listenerManager = player.GetComponent<ListenersManager>();
            listenerManager.FListeners.Remove(this);
            listenerManager.QListeners.Remove(carComponent.GetComponent<RadioSounder>());
            listenerManager.QUpListeners.Remove(carComponent.GetComponent<RadioSounder>());
            carComponent.GetComponent<RadioSounder>().EventUpDid();//выключаем отображение радио вручную, если человек не отжал Q при выходе
            commands.CmdSetRadio(carComponent.gameObject, false);
        }
        PlayerInCar = false;
        playerNet.CmdWakeUp();
        playerInventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
        playerInventoryController.inventories[0].BackInHands();//Возвращаем в руки все
        TurnLabel(NeedShowLabel());
    }

    public void SitIn(bool isDriver, GameObject player)
    {
        if (isDriver)
        {
            carComponent.lastpos = carComponent.transform.position;
            carComponent.playerCommands = commands;
            carComponent.canMove = canDrive = true;
            var listenerManager = player.GetComponent<ListenersManager>();
            listenerManager.FListeners.Add(this);
            listenerManager.QListeners.Add(carComponent.GetComponent<RadioSounder>());
            listenerManager.QUpListeners.Add(carComponent.GetComponent<RadioSounder>());

            commands.CmdSetRadio(carComponent.gameObject, true);
            carComponent.GetComponent<RadioSounder>().Init();

            synComponent._commands = player.GetComponent<Commands>();
            synComponent.isDriver = true;
        }
        PlayerInCar = true;
        playerNet.CmdUnstuck();
        commands.CmdDestroyObjectInHands(player); //Убираем из рук все вещи
        playerInventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
        TurnLabel(NeedShowLabel());
    }

    public override void DisconectExit(string id)
    {
        if (passagers.Contains(id))
        {
            commands.CmdWantToChangeInCar(transform.parent.gameObject, id, false);
        }
    }

    public override bool Do(GameObject player)
    {
        if (passagers.Contains(localPlayerId))
        {
            if (carComponent.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > 1)
                return false;
            TurnLabel(true);
            commands.CmdWantToChangeInCar(transform.parent.gameObject, localPlayerId, false);
        }
        else if (passagers.Count < MaxCount)
        {
            TurnLabel(false);
            commands.CmdWantToChangeInCar(transform.parent.gameObject, localPlayerId, true);
        }
        else
            return false;
        return true;
    }

    public void EventDid()
    {
        commands.CmdSetLightsState(transform.parent.gameObject, !carComponent.LightsState);
    }
}