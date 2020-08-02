using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net.Http.Headers;
using System.Linq;

public class Commands : NetworkBehaviourExtension
{

    [SerializeField] private AudioSource soundsObject;
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] PlayerNet plNet;
    readonly string[] emptyPotionsNames = new string[] { "WaterEmpty", "PyatLEmpty" };

    #region inventory
    GameObject data;
    GameObject Data
    {
        get
        {
            if (data == null)
                data = GetComponent<PlayerNet>().inventoryData;
            return data;
        }
    }
    InventoryData inv0;
    InventoryData Inv0
    {
        get
        {
            if (inv0 == null)
                inv0 = Data.transform.GetComponent<InventoryData>();
            return inv0;
        }
    }
    InventoryData inv1;
    InventoryData Inv1
    {
        get
        {
            if (inv1 == null)
                inv1 = Data.transform.GetChild(1).GetComponent<InventoryData>();
            return inv1;
        }
    }


    [TargetRpc]
    public void TargetForceRequestInventory(NetworkConnection target, GameObject invControllerOject, int invNumber)
    {
        invControllerOject.GetComponent<InventoryController>().inventories[invNumber].RequesData();
    }

    [Command]
    public void CmdRequestInventory(GameObject invControllerOject, int invNumber)
    {
        var data = invControllerOject.GetComponent<InventoryController>().inventories[invNumber].data;
        for (var y = 0; y < data.ROWS; y++)
            for (var x = 0; x < data.COLS; x++)
            {
                var itemData = data.data[y, x];
                if (itemData.Item1 != "")
                    TargetForcePutItem(connectionToClient, invControllerOject, invNumber, itemData.Item1, y, x, itemData.Item2);
            }
    }
    

    [Command]
    public void CmdTakeItem(GameObject item)
    {

        /*        if ((pos = GetComponent<PlayerNet>().inventoryData.GetComponent<InventoryData>().AddItem(item.GetComponent<Item>().PrefabName, 1)).Item1 != -1)
                {
                    TargetTakeItem(connectionToClient, item.GetComponent<Item>().PrefabName, pos.Item1, pos.Item2, pos.Item3, pos.Item4);
                    NetworkServer.Destroy(item);
                }*/
        var itemComp = item.GetComponent<Item>();
        TryPut(itemComp.PrefabName, itemComp.CurrentAmount, new InventoryData[] { Inv1, Inv0 });
        NetworkServer.Destroy(item);
    }

    [Command]
    public void CmdObjectTakeItemName(string item, int count, GameObject controller)
    {
        var contr = controller.GetComponent<InventoryController>();
        var datas = new InventoryData[contr.inventories.Length];
        for (var i = 0; i < contr.inventories.Length; i++)
            datas[i] = contr.inventories[i].data;

        TryPut(item, count, datas, controller);
    }

    [Command]
    public void CmdTakeItemName(string itemName, int count)
    {
        TryPut(itemName, count, new InventoryData[] { Inv1, Inv0 });
    }

    [Command]
    public void CmdThroveDetectingItem(GameObject item, Vector3 position, Quaternion rotation)
    {
        var itemData = item.GetComponent<DetectingItem>().itemData;
        var prName = itemData.PrefabName;
        var amount = itemData.CurrentAmount;
        var gameObject = Instantiate(Resources.Load(prName), position, rotation) as GameObject;
        NetworkServer.Spawn(gameObject);
        NetworkServer.Destroy(item);
        RpcSetObjectCount(gameObject, amount);
    }

    void TryPut(string prName, int count, InventoryData[] inventories, GameObject controllerObj = null)
    {
        (int, int, int, int) pos;
        var item = ((GameObject)Resources.Load(prName));
        if (item == null)
            return;
        var newItemMaxAmount = item.GetComponent<Item>().MaxAmount;

        foreach (var inv in inventories)
        {
            while ((pos = inv.TryAddItemToStack(newItemMaxAmount, prName, count)).Item1 != -1)
            {
                if (controllerObj == null)
                    TargetTakeItem(connectionToClient, prName, pos.Item1, pos.Item2, pos.Item3, pos.Item4);
                else
                    RpcTakeItem(controllerObj, prName, pos.Item1, pos.Item2, pos.Item3, pos.Item4);
                count -= pos.Item4;
                if (count < 1) return;
            }
        }
        foreach(var inv in inventories)
        {
            while ((pos = inv.TryAddItemInEmpty(prName, System.Math.Min(count, newItemMaxAmount))).Item1 != -1)
            {
                if (controllerObj == null)
                    TargetTakeItem(connectionToClient, prName, pos.Item1, pos.Item2, pos.Item3, pos.Item4);
                else
                    RpcTakeItem(controllerObj, prName, pos.Item1, pos.Item2, pos.Item3, pos.Item4);
                count -= pos.Item4;
                if (count < 1) return;
            }
        }
    }

    [Command]
    public void CmdTakeDetectingItem(GameObject item)
    {
        var itemData = item.GetComponent<DetectingItem>().itemData;
        TryPut(itemData.PrefabName, itemData.CurrentAmount, new InventoryData[] {Inv1, Inv0});
        NetworkServer.Destroy(item);
    }

    [TargetRpc]
    void TargetTakeItem(NetworkConnection target, string item, int y, int x, int inventoryNum, int count)
    {
        var itemData = ((GameObject)Resources.Load(item)).GetComponent<Item>().CopyItem();
        GetComponent<InventoryController>().inventories[inventoryNum].cells[y, x].Put(itemData, count);
    }

    [ClientRpc]
    void RpcTakeItem(GameObject invObj, string item, int y, int x, int inventoryNum, int count)
    {
        var itemData = ((GameObject)Resources.Load(item)).GetComponent<Item>().CopyItem();
        invObj.GetComponent<InventoryController>().inventories[inventoryNum].cells[y, x].Put(itemData, count);
    }

    [TargetRpc]
    void TargetPutItem(NetworkConnection target, GameObject invControllerObject, int num, string item, int y, int x, int count)
    {
        //print(invControllerObject.GetComponent<PlayerInventoryController>().inventories[num].name);
        var itemData = ((GameObject)Resources.Load(item)).GetComponent<Item>().CopyItem();
        invControllerObject.GetComponent<InventoryController>().inventories[num].cells[y, x].Put(itemData, count);
    }

    [TargetRpc]
    void TargetForcePutItem(NetworkConnection target, GameObject invControllerObject, int num, string item, int y, int x, int count)
    {
        //print(invControllerObject.GetComponent<PlayerInventoryController>().inventories[num].name);
        var itemData = ((GameObject)Resources.Load(item)).GetComponent<Item>().CopyItem();
        invControllerObject.GetComponent<InventoryController>().inventories[num].cells[y, x].PutForce(itemData, count);
    }

    [Command]
    public void CmdThroveItem(GameObject invObj, int num, string item, int y, int x, int count, Vector3 position, Quaternion rotation, GameObject owner)
    {
        if (invObj.GetComponent<InventoryController>().inventories[num].data.RemoveItem(y, x, count))
        {
            TargetRemoveItem(connectionToClient, invObj, num, item, y, x, count);
            var gameObject = Instantiate(Resources.Load(item), position, rotation) as GameObject;
            NetworkServer.Spawn(gameObject);
            RpcSetObjectCount(gameObject, count);
            RpcSetItemOwner(gameObject, owner);
        }
    }

    [ClientRpc]
    void RpcSetItemOwner(GameObject item, GameObject owner)
    {
        item.GetComponent<ICanBeOwn>().Owner = owner;
    }

    [ClientRpc]
    void RpcSetObjectCount(GameObject item, int count)
    {
        item.GetComponent<Item>().CurrentAmount = count;
    }

    [Command]
    public void CmdReplaceItem(GameObject invObj, GameObject invObj2, int num, int num2, string item, int y, int x, int count, int y2, int x2)
    {
        if (invObj.GetComponent<InventoryController>().inventories[num].data.RemoveItem(y, x, count))
        {
            var invData2 = invObj2.GetComponent<InventoryController>().inventories[num2].data;
            //print(invData2.data[y2, x2].Item2);
            var data2 = invData2.AddItemWithPos(item, count, y2, x2);
            TargetRemoveItem(connectionToClient, invObj, num, item, y, x, count);
            //print(invData2.data[y2, x2].Item2);
            TargetForcePutItem(connectionToClient, invObj2, num2, item, y2, x2, invData2.data[y2, x2].Item2);
            if (data2.Item1 != "")
            {
                invObj.GetComponent<InventoryController>().inventories[num].data.AddItemWithPos(data2.Item1, data2.Item2, y, x);
                TargetPutItem(connectionToClient, invObj, num, data2.Item1, y, x, data2.Item2);
            }
        }
    }

    [Command]
    public void CmdFillAllPotions(int num, int y, int x)
    {
        var controller = gameObject.GetComponent<InventoryController>();
        var data = controller.inventories[num];

        //print(num + " : " + y + " : " + x + " : " + data.data.data[y, x].Item1);

        if (emptyPotionsNames.Contains(data.data.data[y, x].Item1))
        {
            var p = Resources.Load(data.data.data[y, x].Item1) as GameObject;
            var potion = p.GetComponent<Potion>();
            var count = data.data.data[y, x].Item2;
            //TargetRemoveItem(connectionToClient, gameObject, n, data.data.data[y, x].Item1, y, x, count);
            data.data.RemoveItem(y, x, count);
            data.data.AddItemWithPos(potion.fullPrefabName, count, y, x);
            TargetForcePutItem(connectionToClient, gameObject, num, potion.fullPrefabName, y, x, count);
        }
        /*        else
                    print(num + " : " + data.data.data[y, x].Item1);*/
    }

    [Command]
    public void CmdUseItem(GameObject invObj, int num, string item, int y, int x, int count)
    {
        var p = Resources.Load(invObj.GetComponent<InventoryController>().inventories[num].cells[y, x].ItemName) as GameObject;
        var potion = p.GetComponent<Potion>();
        if (potion.isEmpty)
            return;
        var newPotionName = potion.emptyPrefabName;
        TargetUseItem(connectionToClient, invObj, num, item, y, x, count);
        invObj.GetComponent<InventoryController>().inventories[num].data.RemoveItem(y, x, count);
        if (newPotionName != null && newPotionName != string.Empty)
        {
            TargetForcePutItem(connectionToClient, invObj, num, newPotionName, y, x, count);
            invObj.GetComponent<InventoryController>().inventories[num].data.AddItemWithPos(newPotionName, count, y, x);
            //print(invObj.GetComponent<PlayerInventoryController>().inventories[num].data.data[y,x].Item1);
        }
        else
        {
            TargetRemoveItem(connectionToClient, invObj, num, item, y, x, count);
        }
        RpcPlaySound(2);
    }

    [TargetRpc]
    void TargetRemoveItem(NetworkConnection target, GameObject invObj, int num, string item, int y, int x, int count)
    {
        invObj.GetComponent<InventoryController>().inventories[num].cells[y, x].Remove(count);
    }

    [TargetRpc]
    void TargetUseItem(NetworkConnection target, GameObject invObj, int num, string item, int y, int x, int count)
    {
        var bar = gameObject.GetComponent<HealthBar>();
        var p = Resources.Load(invObj.GetComponent<InventoryController>().inventories[num].cells[y, x].ItemName) as GameObject;
        var potion = p.GetComponent<Potion>();
        bar.AddEnergy(potion.EnergyAdd);
        bar.AddWater(potion.WaterAdd);
        //RemoveItem(invObj, num, item, y, x, count);
    }

    [Command]
    public void CmdSpawnObject(string prefab, Vector3 position, Quaternion rotation)
    {
        var gameObject = Instantiate(Resources.Load(prefab), position, rotation) as GameObject;
        NetworkServer.Spawn(gameObject);
    }


    #endregion

    #region car
    [Command]
    public void CmdChangeRadio(GameObject obj, int num)
    {
        RpcChangeRadio(obj, num);
    }

    [ClientRpc]
    void RpcChangeRadio(GameObject obj, int num)
    {
        obj.GetComponent<RadioSounder>().ChangeRadioEverywere(num);
    }

    /*    public void SendMusic(GameObject obj, GameObject objRadio)
        {
            var radio = objRadio.GetComponent<OnceRadio>().GetMusicNums();
            RpcSendMusic(obj, radio.Item1, radio.Item2);
        }*/

    public void SendMusic2(GameObject obj, int num, float timer)
    {
        RpcSendMusic(obj, num, timer);
    }

    [ClientRpc]
    void RpcSendMusic(GameObject obj, int num, float timer)
    {
        obj.GetComponent<RadioSounder>().TakeMusic(num, timer);
    }

    [Command]
    public void CmdSetRadio(GameObject objRadio, bool state)
    {
        RpcSetRadio(objRadio, state);
    }

    [ClientRpc]
    void RpcSetRadio(GameObject objRadio, bool state)
    {
        objRadio.GetComponent<RadioSounder>().Set(state);
    }

    [Command]
    public void CmdWantToChangeInCar(GameObject carArea, string playerId, bool wantSit)
    {
        var area = carArea.GetComponentInChildren<CarAreaDoing>();
        var player = taskManager.GetPlayerData(playerId).playerObject;

        if (wantSit)
        {
            if (area.passagers.Contains(playerId))
                return;
            if (area.Driver == null && (area.owner == null || area.owner == player))
            {
                area.Driver = playerId;//сразу приваиваем, чтобы никто не успел стать драйвером еще раз
                RpcAddPassager(carArea, playerId, true, player);
                RpcSetLightsState(carArea, true);
                RpcSetCurrentParticlesState(carArea, true);
                RpcClearCarParticleObject(carArea);
                RpcSetCurrentParticlesActive(carArea, true);
            }
            else
                RpcAddPassager(carArea, playerId, false, player);
        }
        else
        {
            if (!area.passagers.Contains(playerId))
                return;
            bool wasDriver = area.Driver == playerId;
            RpcRemovePassager(carArea, playerId, wasDriver, player);
            if (area.passagers.Count == 1)
            {
                RpcSetCurrentParticlesState(carArea, false);
                RpcSetCurrentParticlesActive(carArea, false);
            }
            if (wasDriver)
            {
                RpcSetStopLights(carArea, false);
                RpcSetLightsState(carArea, false);
            }
        }
    }

    [Command]
    public void CmdSetStopLights(GameObject carArea, bool state)
    {
        RpcSetStopLights(carArea, state);
    }

    [ClientRpc]
    void RpcSetStopLights(GameObject carArea, bool state)
    {
        carArea.GetComponentInChildren<CarAreaDoing>().carComponent.SetStopLights(state);
    }

    public void SetStopLights(GameObject carArea, bool state)
    {
        TargetSetStopLights(connectionToClient, carArea, state);
    }

    [TargetRpc]
    void TargetSetStopLights(NetworkConnection target, GameObject carArea, bool state)
    {
        carArea.GetComponent<CarMoving>().SetStopLights(state);
    }

    [Command]
    public void CmdSetCarCurrentDirection(GameObject carArea, float x, float y)
    {
        RpcSetCarCurrentDirection(carArea, x, y);
    }

    public void SetCarCurrentDirection(GameObject carArea, float x, float y)
    {
        TargetSetCarCurrentDirection(connectionToClient, carArea, x, y);
    }

    [ClientRpc]
    void RpcSetCarCurrentDirection(GameObject carArea, float x, float y)
    {
        carArea.GetComponentInChildren<CarAreaDoing>().carComponent.CurrentDirection = new Vector2(x, y);
    }

    [TargetRpc]
    void TargetSetCarCurrentDirection(NetworkConnection target, GameObject carArea, float x, float y)
    {
        carArea.GetComponentInChildren<CarAreaDoing>().carComponent.CurrentDirection = new Vector2(x, y);
    }

    [Command]
    public void CmdSetCarState(GameObject carArea, int state, float wheelsSpeed)
    {
        RpcSetCarState(carArea, state, wheelsSpeed);
    }

    public void ThrowCarState(GameObject carArea, int state, float wheelsSpeed)
    {
        TargetSetCarState(connectionToClient, carArea, state, wheelsSpeed);
    }

    [ClientRpc]
    void RpcSetCarState(GameObject carArea, int state, float wheelsSpeed)
    {
        carArea.GetComponent<CarMoving>().SetState(state, wheelsSpeed);
    }

    [TargetRpc]
    void TargetSetCarState(NetworkConnection target, GameObject carArea, int state, float wheelsSpeed)
    {
        carArea.GetComponent<CarMoving>()._animWheels.Play(carArea.GetComponent<CarMoving>().motionName + state);
        carArea.GetComponent<CarMoving>().Init();
        carArea.GetComponent<CarMoving>().SetState(state, wheelsSpeed);
    }

    [ClientRpc]
    void RpcClearCarParticleObject(GameObject carArea)
    {
        ParticleSystem p1;
        if (p1 = carArea.GetComponent<CarMoving>().currentParticles.GetComponent<ParticleSystem>())
        {
            p1.Clear();//вызвать у всех
        }
    }

    [ClientRpc]
    void RpcSetCurrentParticlesState(GameObject carArea, bool state)
    {
        carArea.GetComponentInChildren<CarAreaDoing>().carComponent.ParticlesState = state;
    }

    [ClientRpc]
    void RpcSetCurrentParticlesActive(GameObject carArea, bool state)
    {
        carArea.GetComponentInChildren<CarAreaDoing>().carComponent.currentParticles.SetActive(state);
    }

    [Command]
    public void CmdSetLightsState(GameObject carArea, bool state)
    {
        RpcSetLightsState(carArea, state);
    }

    [ClientRpc]
    void RpcSetLightsState(GameObject carArea, bool state)
    {
        carArea.GetComponent<CarMoving>().SetLightsState(state);
    }

    public void SetLightsState(GameObject carArea, bool state)
    {
        TargetSetLightsState(connectionToClient, carArea, state);
    }

    [TargetRpc]
    void TargetSetLightsState(NetworkConnection target, GameObject carArea, bool state)
    {
        carArea.GetComponent<CarMoving>().SetLightsState(state);
    }

    [ClientRpc]
    void RpcAddPassager(GameObject car, string newPassager, bool isDriver, GameObject passager)
    {
        car.GetComponentInChildren<CarAreaDoing>().AddOne(newPassager, isDriver, passager);
    }

    [TargetRpc]
    public void TargetAddPassager(NetworkConnection target, GameObject car, string newPassager, bool isDriver, GameObject passager)
    {
        car.GetComponentInChildren<CarAreaDoing>().AddOne(newPassager, isDriver, passager);
    }

    [ClientRpc]
    void RpcRemovePassager(GameObject car, string newPassager, bool isDriver, GameObject passager)
    {
        car.GetComponentInChildren<CarAreaDoing>().RemoveOne(newPassager, isDriver, passager);
    }

    #endregion

    #region sleep

    [Command]
    public void CmdReadyToSkip(string id, int time, GameObject sleepArea, int spriteNum)
    {
        RpcReadyToSkip(id, time, sleepArea, spriteNum);
        if (time != -1)
            taskManager.ReadyToSkip(id, time);
    }

    [Command]
    public void CmdReadyToSkipWithoutSprite(string id, int time)
    {
        if (time != -1)
            taskManager.ReadyToSkip(id, time);
    }

    [ClientRpc]
    public void RpcReadyToSkip(string id, float time, GameObject sleepArea, int spriteNum)
    {
        var spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + spriteNum);
        sleepArea.GetComponentInChildren<SleepAreaDoing>().AddOne(id, spritesHead[3]);
    }

    [Command]
    public void CmdWakeUp(GameObject sleepArea, string guid, bool needReturnItems)
    {
        WakeUp(sleepArea, guid, needReturnItems);
    }

    public void WakeUp(GameObject sleepArea, string guid, bool needReturnItems, bool removing = false)
    {
        RpcWakeUp(sleepArea, guid, needReturnItems);
        taskManager.UnReadyToSkip(guid, removing);
    }

    [ClientRpc]
    public void RpcWakeUp(GameObject sleepArea, string sleeper, bool needReturnItems)
    {
        sleepArea.GetComponentInChildren<SleepAreaDoing>().RemoveOne(sleeper, needReturnItems);
    }

    #endregion

    #region mobs

    public void SendAnimState(GameObject obj)
    {
        TargetSendAnims(connectionToClient, obj, obj.GetComponent<StandartMoving>().frontState, obj.GetComponent<StandartMoving>().isShifted);
    }

    [Command]
    public void CmdSetMovingVanish(GameObject obj, bool state)
    {
        RpcSetMovingVanish(obj, state);
    }

    [ClientRpc]
    void RpcSetMovingVanish(GameObject obj, bool state)
    {
        SetRenders(obj, state);
    }

    [TargetRpc]
    public void TargetSetVanish(NetworkConnection target, GameObject obj, bool state)
    {
        SetRenders(obj, state);
    }

    void SetRenders(GameObject obj, bool state)
    {
        obj.GetComponent<StandartMoving>().SetAllRenders(state);
    }

    [TargetRpc]
    void TargetSendAnims(NetworkConnection target, GameObject obj, int state, bool shifted)
    {
        obj.GetComponent<StandartMoving>().SendAllData(state, shifted);
    }

    [Command]
    public void CmdSetMobParts(GameObject mob)
    {
        var a = mob.GetComponent<MobController>();
        RpcSetMobParts(mob, a.x, a.y, a.z, a.male, a.LabelName);
    }

    [ClientRpc]
    public void RpcSetMobParts(GameObject mob, int x, int y, int z, bool male, string newName)
    {
        mob.GetComponent<MobController>().InitParts(x, y, z, male, newName);
    }

    [Command]
    public void CmdSendPosition(Vector3 pos, GameObject mob)
    {
        RpcSendPosition(pos, mob);
    }

    [ClientRpc]
    public void RpcSendPosition(Vector3 pos, GameObject mob)
    {
        mob.GetComponent<Synchronization>()._lastPosition = pos;
    }

    [Command]
    public void CmdSendScale(Vector3 scale, GameObject mob)
    {
        RpcSendScale(scale, mob);
    }

    [ClientRpc]
    public void RpcSendScale(Vector3 scale, GameObject mob)
    {
        mob.GetComponent<Synchronization>()._lastScale = scale;
    }

    [Command]
    public void CmdSetMobAnimation(GameObject mob, int x, int y, float speed, bool shifted)
    {
        RpcSetMobAnimation(mob, x, y, speed, shifted);
    }

    [ClientRpc]
    public void RpcSetMobAnimation(GameObject mob, int x, int y, float speed, bool shifted)
    {
        mob.GetComponent<MobController>().SetAnim(x, y, speed, shifted);
    }

    [Command]
    public void CmdDestroyObjectInHands(GameObject movingObject)
    {
        RpcDestroyObjectInHands(movingObject);
    }

    [Command]
    public void CmdSetScaleInHands(GameObject movingObj, float time, int energy, int num)
    {
        RpcSetScaleInHands(movingObj, time, energy, num);
    }

    [ClientRpc]
    void RpcSetScaleInHands(GameObject movingObj, float time, int energy, int num)
    {
        if (movingObj == gameObject && isLocalPlayer) return;
        movingObj.GetComponent<StandartMoving>().SetScaleInHands(time, energy, false, num);
    }

    [Command]
    public void CmdDestroyScaleInHands(GameObject movingObj)
    {
        RpcDestroyScaleInHands(movingObj);
    }

    [ClientRpc]
    void RpcDestroyScaleInHands(GameObject movingObject)
    {
        if (movingObject == gameObject && isLocalPlayer) return;
        movingObject.GetComponent<StandartMoving>().DestroyScaleInHands();
    }

    [Command]
    public void CmdRemoveObjectsInHands(GameObject movingObject, string[] obj)
    {
        if (obj.Length == 0) return;
        RpcRemoveObjectsInHands(movingObject, obj);
    }

    [ClientRpc]
    void RpcRemoveObjectsInHands(GameObject movingObject, string[] obj)
    {
        movingObject.GetComponent<StandartMoving>().RemoveObjectsInHands(obj);
    }

    [Command]
    public void CmdEnterInWater(GameObject movingObject, int deep)
    {
        EnterInWater(movingObject, deep);
    }

    public void SendEnterInWater(GameObject movingObject)
    {
        var mov = movingObject.GetComponent<StandartMoving>();
        TargetEnterInWater(connectionToClient, movingObject, mov.waterLevel);
    }

    public void SendMobData(GameObject movingObject)
    {
        var mob = movingObject.GetComponent<MobController>();
        TargetSetMobData(connectionToClient, movingObject, mob.male, mob.mobName);
    }

    [TargetRpc]
    public void TargetSetMobData(NetworkConnection target, GameObject movingObject, bool male, string mobName)
    {
        movingObject.GetComponent<MobController>().SetData(male, mobName);
    }

    [TargetRpc]
    public void TargetEnterInWater(NetworkConnection target, GameObject movingObject, int deep)
    {
        movingObject.GetComponent<StandartMoving>().EnterInWater(deep);
    }

    public void EnterInWater(GameObject movingObject, int deep)
    {
        RpcEnterInWater(movingObject, deep);
        if (deep != 0)
        {
            RpcDestroyObjectInHands(movingObject); //Убираем из рук все вещи
        }
    }

    [ClientRpc]
    public void RpcEnterInWater(GameObject movingObject, int deep)
    {
        movingObject.GetComponent<StandartMoving>().EnterInWater(deep);
    }

    [ClientRpc]
    void RpcDestroyObjectInHands(GameObject movingObject)
    {
        movingObject.GetComponent<StandartMoving>().DestroyOb();
    }

    [Command]
    public void CmdSpawnObjectInHands(GameObject movingObject, string[] objs)
    {
        RpcSpawnObjectInHands(movingObject, objs);
    }

    [ClientRpc]
    void RpcSpawnObjectInHands(GameObject movingObject, string[] objs)
    {
        movingObject.GetComponent<StandartMoving>().SetObjectInHandsO(objs);
    }

    public void SendSpawnObjectInHands(GameObject movingObject)
    {
        TargetSpawnObjectInHands(connectionToClient, movingObject, movingObject.GetComponent<StandartMoving>().inHandsNames.ToArray());
    }

    [TargetRpc]
    void TargetSpawnObjectInHands(NetworkConnection target, GameObject movingObject, string[] objs)
    {
        movingObject.GetComponent<StandartMoving>().SetObjectInHandsO(objs);
    }

    [Command]
    public void CmdChangeInHandsPosition(GameObject movingObject, string name, bool state)
    {
        RpcChangeInHandsPosition(movingObject, name, state);
    }

    [ClientRpc]
    void RpcChangeInHandsPosition(GameObject movingObject, string name, bool state)
    {
        movingObject.GetComponent<StandartMoving>().ChangeInHandsPosition(name, state);
    }

    #endregion

    #region digging

    [Command]
    public void CmdSpawnFindObject(string prefab, Vector3 position, Quaternion rotation, GameObject parent, int depth)
    {
        var gameObject = Instantiate(Resources.Load(prefab), position, rotation) as GameObject;
        //gameObject.transform.parent = parent.transform;
        NetworkServer.Spawn(gameObject);
        RpcInitWhole(gameObject, depth, parent);
    }

    [ClientRpc]
    void RpcInitWhole(GameObject obj, int depth, GameObject parent)
    {
        var a = obj.GetComponent<DetectingItem>();
        if (a == null) print(111);
        a.init(depth, parent);
    }

    #endregion


    [Command]
    public void CmdSetCampEnter(GameObject camp, bool state)
    {
        RpcSetCampEnter(camp, state);
    }

    [ClientRpc]
    public void RpcSetCampEnter(GameObject camp, bool state)
    {
        camp.GetComponent<CampEnterAreaDoing>().SetState(state);
    }

    [Command]
    public void CmdAddMessege(string text, bool needDestroyPrevious)
    {
        RpcAddMessege(text, needDestroyPrevious);
    }

    [ClientRpc]
    public void RpcAddMessege(string text, bool needDestroyPrevious)
    {
        GetComponent<HeadMessegesManager>().AddMessege(text, needDestroyPrevious);
    }

    [Command]
    public void CmdPrintEverywhere(string text)
    {
        RpcPrintEverywhere(text);
    }

    [ClientRpc]
    void RpcPrintEverywhere(string text)
    {
        print(text);
    }

    [Command]
    public void CmdSkipTime(int ToTime, int multiplayer)
    {
        taskManager.SkipTo(ToTime, multiplayer);
    }

    [Command]
    public void CmdConnected()
    {
        GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>().Connected();
    }

    [Command]
    public void CmdDestroyObject(GameObject needToDestroy)
    {
        NetworkServer.Destroy(needToDestroy);
    }

    [ClientRpc]
    public void RpcSetEnergyMultiplayer(int multiplayer)
    {
        GameObject.Find("LocalPlayer").GetComponent<HealthBar>().multiplayer = multiplayer;
    }

    [Command]
    public void CmdPlaySound(int num)
    {
        RpcPlaySound(num);
    }

    [ClientRpc]
    public void RpcPlaySound(int num)
    {
        soundsObject.PlayOneShot(sounds[num]);
    }

    #region TaskRegion

    [Command]
    public void CmdMiniGamePlus(GameObject controller, int number)
    {
        //print("cmd");
        //RpcMiniGamePlus(controller, number);
        RpcMiniGameSet(controller, number, controller.GetComponent<TaskControllerScript>()._minigame[number].currentCount + 1);
    }

    [ClientRpc]
    public void RpcMiniGamePlus(GameObject controller, int number)
    {
        //Старый метод, теперь используется RpcMiniGameSet()
        controller.GetComponent<TaskControllerScript>()._minigame[number].AddOne();
    }

    [ClientRpc]
    public void RpcMiniGameSet(GameObject controller, int number, int count)
    {
        //TODO вызывается два раза, хуй пойми почему
        controller.GetComponent<TaskControllerScript>()._minigame[number].SetCount(count);
    }

    [Command]
    public void CmdSetPlan(GameObject controller, int numOfPlan)
    {
        RpcSetPlan(controller, numOfPlan);
    }

    [ClientRpc]
    public void RpcSetPlan(GameObject controller, int numOfPlan)
    {
        controller.GetComponent<TaskControllerScript>().ChangePlan(numOfPlan);
    }

    [Command]
    public void CmdStartGames(GameObject controller)
    {
        RpcStartGames(controller);
    }

    [ClientRpc]
    public void RpcStartGames(GameObject controller)
    {
        controller.GetComponent<TaskControllerScript>().StartGames();
    }

    [Command]
    public void CmdInitTaskController(GameObject taskObject, int type, bool needMenu, bool needButtons, string name, int[] stages)
    {
        RpcInitTaskController(taskObject, type, needMenu, needButtons, name, stages);
    }

    [ClientRpc]
    void RpcInitTaskController(GameObject taskObject, int type, bool needMenu, bool needButtons, string name, int[] stages)
    {
        var taskController = taskObject.GetComponent<TaskControllerScript>();//cooker, digger итд
        taskController.Reinitializate(type, needMenu, needButtons, name, stages);
    }
    #endregion
}
