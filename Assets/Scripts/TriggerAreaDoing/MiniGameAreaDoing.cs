using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameAreaDoing : TriggerAreaDoing
{
    public override bool NeedShowLabel()
    {
        //var a = GetComponent<MiniGameController>();
        return PlayerThere;
    }

    public override bool Do()
    {
        GetComponent<MiniGameController>().SpawnMiniGame();
        return true;
    }
}