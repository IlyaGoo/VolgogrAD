using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskControllerAreaDoing : TriggerAreaDoing
{
    [SerializeField] private TaskControllerScript thisTaskControllerScript;
    public override bool NeedShowLabel()
    {
        return thisTaskControllerScript.isOn && PlayerThere;
    }

    public override bool Do()
    {
        localCommands.CmdStartGames(gameObject);
        return true;
    }
}