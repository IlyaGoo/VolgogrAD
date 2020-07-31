using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : NetworkBehaviour
{
    [SerializeField] PlayerNet playerNet;
    [SerializeField] Commands cmd;
    public TaskManager Task => playerNet.Task;


    [Command]
    public void CmdRequestPlayersDatas()
    {
        foreach (var pl in Task.PlayersDatas.Values)
        {
            playerNet.SendSpawnParts(pl.playerObject);
            cmd.SendEnterInWater(pl.playerObject);
            playerNet.SendNickname(pl.playerObject);
            cmd.SendSpawnObjectInHands(pl.playerObject);
            cmd.SendAnimState(pl.playerObject);
            cmd.TargetSetVanish(connectionToClient, pl.playerObject, !pl.playerObject.GetComponent<StandartMoving>().vanished);
        }
    }

    [Command]
    void CmdRequestMobsDatas()
    {
        foreach(var mob in Task._mobManager.mobs)
        {
            playerNet.SendSpawnParts(mob.gameObject);
            cmd.SendEnterInWater(mob.gameObject);
            cmd.SendMobData(mob.gameObject);
            cmd.SendSpawnObjectInHands(mob.gameObject);
            cmd.SendAnimState(mob.gameObject);
            cmd.TargetSetVanish(connectionToClient, mob.gameObject, !mob.GetComponent<StandartMoving>().vanished);
        }
    }

    [Command]
    void CmdRequestCarsDatas()
    {
        foreach(var car in Task.carsAreas)
        {
            foreach(var passager in car.passagers)
            {
                cmd.TargetAddPassager(connectionToClient, car.transform.parent.gameObject, passager, passager.Equals(car.Driver), Task.GetPlayerData(passager).playerObject);
            }
            cmd.ThrowCarState(car.transform.parent.gameObject, car.carComponent.currentState, car.carComponent._animWheels.speed);
            cmd.SetCarCurrentDirection(car.transform.parent.gameObject, car.carComponent.CurrentDirection.x, car.carComponent.CurrentDirection.y);
            cmd.SetStopLights(car.transform.parent.gameObject, car.carComponent.stopLightActive);
            cmd.SetLightsState(car.transform.parent.gameObject,  car.carComponent.LightsState);
        }
    }

    public void RequestAll()
    {
        CmdRequestPlayersDatas();
        CmdRequestRadio();
        CmdRequestSleepers();
        CmdRequestMobsDatas();
        CmdSetLight();
        CmdRequestCamps();
        CmdRequestCarsDatas();
    }

    [Command]
    void CmdRequestRadio()
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

    [Command]
    void CmdRequestSleepers()
    {
        foreach (var sleepArea in FindObjectsOfType<SleepAreaDoing>())
        {
            if (sleepArea.sleepers.Count > 0)
            {
                foreach(var sleeper in sleepArea.sleepers)
                {
                    TargetReadyToSkip(connectionToClient, sleepArea.gameObject, Task.PlayersDatas[sleeper].HeadNum, sleeper);
                }
            }
        }
    }

    [TargetRpc]
    public void TargetReadyToSkip(NetworkConnection target, GameObject sleepArea, int spriteNum, string sleeper)
    {
        var spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + spriteNum);
        sleepArea.GetComponentInChildren<SleepAreaDoing>().AddOne(sleeper, spritesHead[3]);
    }

    [TargetRpc]
    void TargetSetRadio(NetworkConnection target, GameObject objRadio, int num, float timer, int radioNum)
    {
        var radio = objRadio.GetComponent<RadioSounder>();
        radio.Set(true);
        radio.ChangeRadioEverywere(radioNum);
        radio.TakeMusic(num, timer);
    }

    [Command]
    public void CmdSetLight()
    {
        TargetSendLight(connectionToClient, Task.globalLight.intensity, Task.currentLightLevel);
    }

    [TargetRpc]
    void TargetSendLight(NetworkConnection target, float intensy, float secondIntensy)
    {
        Task.SetConnectionLight(intensy, secondIntensy);
    }

    [Command]
    public void CmdRequestCamps()
    {
        foreach (var camp in Task.campsAreas)
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
