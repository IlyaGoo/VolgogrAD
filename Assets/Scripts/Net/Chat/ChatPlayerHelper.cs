using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using System.Globalization;

public class ChatPlayerHelper : NetworkBehaviourExtension { 
    public void Send(string nickname, string message)
    {
        var id = GetComponent<NetworkIdentity>().netId.ToString();
        CmdSend(id, nickname, message);
    }
	
    [Command]
    public void CmdSend(string id, string nickname, string message)
    {
        if (message[0] == '/' && playerMetaData.cheatsAavable)
        {
            string[] command = message.Substring(1).Split(' ');
            switch (command[0].ToLower())
            {
                case "time":
                    if (command.Length < 2) break;
                    int time;
                    var timePart = command[1].Split(':');
                    if (timePart.Length == 1)
                        try { time = int.Parse(command[1]); }
                        catch (FormatException)
                        {
                            break;
                        }
                    else
                    {
                        try
                        {
                            time = int.Parse(timePart[0]) * 60 + int.Parse(timePart[1]);
                            if (Timer.instance.gameTimer > time)
                                time += 1439;//нужно пересечь ночь, которая отнимает это кол-во
                        }
                        catch (FormatException)
                        {
                            break;
                        }
                    }
                    if (time < 0) break;
                    GetComponent<Commands>().CmdSkipTime(time, 800);
                    break;
                case "give":
                    if (command.Length < 2) break;
                    int count = 1;
                    if (command.Length > 2)
                    {
                        try
                        {
                            count = int.Parse(command[2]);
                        }
                        catch { }
                    }
                    GetComponent<Commands>().CmdTakeItemName(command[1], count);
                    break;
                case "speed":
                    if (command.Length < 2) break;
                    float newSpeed = 3;
                    try
                    {
                        newSpeed = float.Parse(command[1], CultureInfo.InvariantCulture);
                    }
                    catch {}
                    GetComponent<Moving>().maxSpeed = newSpeed;
                    break;
                case "private":
                    if (command.Length < 2) break;
                    if (command[1] == "true")
                        RpcSetPrivate(true);
                    else if (command[1] == "false")
                        RpcSetPrivate(false);
                    break;
            }
        }
        RpcSend(id, nickname, message);
    }

    [ClientRpc]
    public void RpcSetPrivate(bool newState)
    {
        playerMetaData.privateEnable = newState;
    }

    [ClientRpc]
    public void RpcSend(string id, string nickname, string message)
    {
        chatHelper.AddMessage(id, nickname, message);
    }

    [ClientRpc]
    public void RpcSendLine(string message)
    {
        chatHelper.AddLine(message);
    }

}
