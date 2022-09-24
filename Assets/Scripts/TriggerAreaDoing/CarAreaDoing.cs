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

    protected override Transform GetPanelParent() => transform.parent;//Иначе при повороте надпись будет крутиться

    public string Driver 
    {
        get => driver;
        set 
        { 
            driver = value;
            var data = EntitysController.instance.GetPlayerData(value);
            driverObject = data == null ? null : EntitysController.instance.GetPlayerData(value).EntityObject;
        } 
    }

    public override bool NeedShowLabel()
    {
        return !PlayerInCar && PlayerThere;
    }

    public override bool CanInteract(GameObject interactEntity)
    {
        return owner == null || owner == interactEntity || owner.CompareTag("Player") && !playerMetaData.privateEnable;
    }

    public void AddOne(string newPassagerId, bool isDriver, GameObject newPassager)
    {
        if (passagers.Count == 0)
        {
            motor.Play();
        }
        localPlayerNet.AddDoing(newPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
        newPassager.GetComponent<CircleCollider2D>().enabled = false;
        if (isDriver)
            Driver = newPassagerId;
        carComponent.passagers.Add(newPassager);
        passagers.Add(newPassagerId);
        RefreshStateLabel();

        if (localPlayer == newPassager)
            SitIn(isDriver);

        newPassager.transform.parent = transform.parent;
        newPassager.transform.position = transform.position + CarMoving.passagersOffset;
    }

    public void RemoveOne(string removingPassagerId, bool isDriver, GameObject removingPassager)
    {
        if (removingPassager == null)
            carComponent.ClearMissingsPassagers();
        else  //Когда убираем дисконектнувшегося игрока, ничего возвращать не нужно
        {
            localPlayerNet.RemoveDoing(removingPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Убираем из player net текущее действие
            removingPassager.GetComponent<CircleCollider2D>().enabled = true;
            removingPassager.transform.parent = null;
        }
        
        passagers.Remove(removingPassagerId);
        if (passagers.Count == 0)
        {
            motor.Stop();
        }
        carComponent.passagers.Remove(removingPassager);

        RefreshStateLabel();
        if (isDriver)
            Driver = null;

        if (localPlayer == removingPassager)
            SitOut(isDriver);
    }

    public void SitOut(bool isDriver)
    {
        if (isDriver)
        {
            carComponent.canMove = canDrive = false;
            synComponent.isDriver = false;
            localListenersManager.FListeners.Remove(this);
            localListenersManager.QListeners.Remove(carComponent.GetComponent<RadioSounder>());
            localListenersManager.QUpListeners.Remove(carComponent.GetComponent<RadioSounder>());
            carComponent.GetComponent<RadioSounder>().EventUpDid();//выключаем отображение радио вручную, если человек не отжал Q при выходе
            localCommands.CmdSetRadio(carComponent.gameObject, false);
        }
        PlayerInCar = false;
        localPlayerNet.CmdWakeUp();
        localPlayerInventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
        localPlayerInventoryController.inventories[0].BackInHands();//Возвращаем в руки все
        RefreshStateLabel();
    }

    public void SitIn(bool isDriver)
    {
        if (isDriver)
        {
            carComponent.lastpos = carComponent.transform.position;
            carComponent.canMove = canDrive = true;
            localListenersManager.FListeners.Add(this);
            localListenersManager.QListeners.Add(carComponent.GetComponent<RadioSounder>());
            localListenersManager.QUpListeners.Add(carComponent.GetComponent<RadioSounder>());

            localCommands.CmdSetRadio(carComponent.gameObject, true);
            carComponent.GetComponent<RadioSounder>().Init();

            synComponent._commands = localCommands;
            synComponent.isDriver = true;
        }
        PlayerInCar = true;
        localPlayerNet.CmdUnstuck();
        localCommands.CmdDestroyObjectInHands(localPlayer); //Убираем из рук все вещи
        localPlayerInventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
        RefreshStateLabel();
    }

    public override void DisconnectExit(string id)
    {
        if (passagers.Contains(id))
        {
            localCommands.CmdWantToChangeInCar(transform.parent.gameObject, id, false);
        }
    }

    public override bool Do()
    {
        if (passagers.Contains(localPlayerId))
        {
            if (carComponent.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > 1)
                return false;
            TurnLabel(true);
            localCommands.CmdWantToChangeInCar(transform.parent.gameObject, localPlayerId, false);
        }
        else if (passagers.Count < MaxCount)
        {
            TurnLabel(false);
            localCommands.CmdWantToChangeInCar(transform.parent.gameObject, localPlayerId, true);
        }
        else
            return false;
        return true;
    }

    public void EventDid()
    {
        localCommands.CmdSetLightsState(transform.parent.gameObject, !carComponent.LightsState);
    }
}