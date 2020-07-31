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
    TaskManager _taskManager;
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
            var data = TaskManagerRef.GetPlayerData(value);
            driverObject = data == null ? null : TaskManagerRef.GetPlayerData(value).playerObject;
        } 
    }

    public TaskManager TaskManagerRef { get 
        {
            if (_taskManager == null)
                _taskManager = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
            return _taskManager;
        } 
        set => _taskManager = value; }

    public override bool NeedShowLabel()
    {
        return !PlayerInCar && PlayerThere;
    }

    public bool CanInteract(GameObject mayBeOwner)
    {
        return owner == null || owner == mayBeOwner || (owner.CompareTag("Player") && !owner.GetComponent<StandartMoving>().PlNet()._meta.privateItems);
    }

    public void AddOne(string newPassagerId, bool isDriver, GameObject newPassager)
    {
        if (passagers.Count == 0)
        {
            motor.Play();
        }
        TaskManagerRef.LocalPlayer.GetComponent<PlayerNet>().AddDoing(newPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Добавляем в player net текущее действие, чтобы при дисконекте встать
        newPassager.GetComponent<CircleCollider2D>().enabled = false;
        if (isDriver)
            Driver = newPassagerId;
        carComponent.passagers.Add(newPassager);
        passagers.Add(newPassagerId);
        TurnLabel(NeedShowLabel());

        if (TaskManagerRef.LocalPlayer == newPassager)
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
            TaskManagerRef.LocalPlayer.GetComponent<PlayerNet>().RemoveDoing(removingPassager.GetComponent<NetworkIdentity>().netId.ToString(), this);//Убираем из player net текущее действие
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

        if (TaskManagerRef.LocalPlayer == removingPassager)
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
            player.GetComponent<Commands>().CmdSetRadio(carComponent.gameObject, false);
        }
        PlayerInCar = false;
        player.GetComponent<PlayerNet>().CmdWakeUp();
        player.GetComponent<PlayerInventoryController>().inventories[0].SetFreeze(false);//Снова включаем панель предметов
        player.GetComponent<PlayerInventoryController>().inventories[0].BackInHands();//Возвращаем в руки все
        TurnLabel(NeedShowLabel());
    }

    public void SitIn(bool isDriver, GameObject player)
    {
        if (isDriver)
        {
            carComponent.lastpos = carComponent.transform.position;
            carComponent.playerCommands = player.GetComponent<Commands>();
            carComponent.canMove = canDrive = true;
            var listenerManager = player.GetComponent<ListenersManager>();
            listenerManager.FListeners.Add(this);
            listenerManager.QListeners.Add(carComponent.GetComponent<RadioSounder>());
            listenerManager.QUpListeners.Add(carComponent.GetComponent<RadioSounder>());

            player.GetComponent<Commands>().CmdSetRadio(carComponent.gameObject, true);
            carComponent.GetComponent<RadioSounder>().Init();

            synComponent._commands = player.GetComponent<Commands>();
            synComponent.isDriver = true;
        }
        PlayerInCar = true;
        player.GetComponent<PlayerNet>().CmdUnstuck();
        player.GetComponent<Commands>().CmdDestroyObjectInHands(player); //Убираем из рук все вещи
        player.GetComponent<PlayerInventoryController>().inventories[0].SetFreeze(true);//Фризим инвентарь
        TurnLabel(NeedShowLabel());
    }

    public override void DisconectExit(string id)
    {
        if (passagers.Contains(id))
        {
            TaskManagerRef.Cmd.CmdWantToChangeInCar(transform.parent.gameObject, id, false);
        }
    }


    public override bool Do(GameObject player)
    {
        Commands cmd = player.GetComponent<Commands>();
        var id = player.GetComponent<NetworkIdentity>().netId.ToString();
        
        if (passagers.Contains(id))
        {
            if (carComponent.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > 1)
                return false;
            TurnLabel(true);
            cmd.CmdWantToChangeInCar(transform.parent.gameObject, id, false);
        }
        else if (passagers.Count < MaxCount)
        {
            TurnLabel(false);
            cmd.CmdWantToChangeInCar(transform.parent.gameObject, id, true);
        }
        else
            return false;
        return true;
    }

    public void EventDid()
    {
        driverObject.GetComponent<Commands>().CmdSetLightsState(transform.parent.gameObject, !carComponent.LightsState);
    }
}