using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChillBaf : Debaf
{
    HealthBar bar;

    HealthBar Bar
    {
        get
        {
            if (bar == null)
                bar = player.GetComponent<HealthBar>();
            return bar;
        }
    }

    public override void Do()
    {
        Bar.AddEnergy(1);
    }
}
