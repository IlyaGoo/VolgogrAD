using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskControllerAreaDoing : TriggerAreaDoing
{
    public override bool NeedShowLabel()
    {
        var a = GetComponent<TaskControllerScript>();
        return a.isOn && PlayerThere;
    }

    public override bool Do(GameObject player)
    {
        player.GetComponent<Commands>().CmdStartGames(gameObject);
        return true;
    }
}