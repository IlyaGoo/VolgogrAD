using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameAreaDoing : TriggerAreaDoing
{
    public override bool NeedShowLabel()
    {
        return false;//return PlayerThere &&  GetComponent<CampObject>().CurrentMiniGameController != null;
    }

    public override bool Do()
    {
        // GetComponent<CampObject>().CurrentMiniGameController.SpawnMiniGame();
        return true;
    }
}