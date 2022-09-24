using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Класс, отвечающий на подключение и синхронизацию при подключении, висит на объекте игрока */
public class Connector : NetworkBehaviourExtension
{
    [SerializeField] Commands cmd;

    public void RequestPlayersData()
    {
        foreach (var pl in EntitysController.instance.playersData.Values)
        {
            cmd.plNet.SendPlayerData(pl);
            cmd.plNet.SendSpawnParts(pl.EntityObject);
            cmd.SendEnterInWater(pl.EntityObject);
            cmd.plNet.SendNickname(pl.EntityObject);
            cmd.SendSpawnObjectInHands(pl.EntityObject);
            cmd.SendAnimState(pl.EntityObject);
            cmd.TargetSetVanish(connectionToClient, pl.EntityObject, !pl.EntityObject.GetComponent<StandartMoving>().vanished);
        }
    }

    void RequestMobsData()
    {
        foreach(var mob in mobsManager.mobs)
        {
            cmd.plNet.SendSpawnParts(mob.gameObject);
            cmd.SendEnterInWater(mob.gameObject);
            cmd.SendMobData(mob.gameObject);
            cmd.SendSpawnObjectInHands(mob.gameObject);
            cmd.SendAnimState(mob.gameObject);
            cmd.TargetSetVanish(connectionToClient, mob.gameObject, !mob.GetComponent<StandartMoving>().vanished);
        }
    }

    void RequestCarsData()
    {
        foreach(var car in taskManager.carsAreas)
        {
            foreach(var passager in car.passagers)
            {
                cmd.TargetAddPassager(
                    connectionToClient, 
                    car.transform.parent.gameObject, 
                    passager, 
                    passager.Equals(car.Driver), 
                    EntitysController.instance.GetPlayerData(passager).EntityObject
                    );
            }
            cmd.ThrowCarState(car.transform.parent.gameObject, car.carComponent.currentState, car.carComponent._animWheels.speed);
            cmd.SetCarCurrentDirection(car.transform.parent.gameObject, car.carComponent.CurrentDirection.x, car.carComponent.CurrentDirection.y);
            cmd.SetStopLights(car.transform.parent.gameObject, car.carComponent.stopLightActive);
            cmd.SetLightsState(car.transform.parent.gameObject,  car.carComponent.LightsState);
        }
    }

    void RequestWholes()
    {
        foreach (var whole in taskManager.wholes)
        {
            cmd.plNet.SendWhole(whole.transform.position);
            cmd.plNet.SendDepth(whole);
        }
    }

    void RequestTime()
    {
        TargetSetTime(connectionToClient, Timer.instance.gameTimer);
    }
    
    [TargetRpc]
    public void TargetSetTime(NetworkConnection target, int time)
    {
        Timer.instance.SetGameTimer(time);
    }

    [Command]
    public void CmdRequestAll()
    {
        RequestTime();
        RequestPlayersData();
        RequestRadio();
        RequestMobsData();
        RequestSleepers();
        SetLight();
        RequestCamps();
        RequestCarsData();
        RequestWholes();
    }

    void RequestRadio()
    {
        foreach(var radio in FindObjectsOfType<RadioSounder>())
        {
            if (radio.isOn)
            {
                var music = radio.currentRadio.GetMusicNums();
                TargetSetRadio(connectionToClient, radio.gameObject, music.Item1, music.Item2, radio.currentRadioNum);
            }
        }
    }

    void RequestSleepers()
    {
        foreach (var sleepArea in FindObjectsOfType<SleepAreaDoing>())
        {
            if (sleepArea.sleepers.Count > 0)
            {
                foreach(var sleeper in sleepArea.sleepers)
                {
                    cmd.TargetReadyToSkip(connectionToClient, sleepArea.transform.parent.gameObject, sleeper.Identity);
                }
            }
        }
    }

    [TargetRpc]
    void TargetSetRadio(NetworkConnection target, GameObject objRadio, int num, float timer, int radioNum)
    {
        var radio = objRadio.GetComponent<RadioSounder>();
        radio.Set(true);
        radio.ChangeRadioEverywere(radioNum);
        radio.TakeMusic(num, timer);
    }

    public void SetLight()
    {
        TargetSendLight(connectionToClient, LightsController.instance.globalLight.intensity, LightsController.instance.currentLightLevel);
    }

    [TargetRpc]
    void TargetSendLight(NetworkConnection target, float intensy, float secondIntensy)
    {
        LightsController.instance.SetConnectionLight(intensy, secondIntensy);
    }

    public void RequestCamps()
    {
        foreach (var camp in taskManager.campsAreas)
        {
            TargetSendCamp(connectionToClient, camp.gameObject, camp.isOpen);
        }
        
    }

    [TargetRpc]
    void TargetSendCamp(NetworkConnection target, GameObject camp, bool state)
    {
        camp.GetComponent<CampEnterAreaDoing>().SetState(state, false);
    }
}
