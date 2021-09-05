using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAreaDoing : TriggerAreaDoing
{
    [SerializeField] Inventory inv;

    public override bool Do()
    {
/*        var cont = player.GetComponent<PlayerNet>();
        cont.invPanelLeft.SetActive(true);
        cont.invPanelRight.SetActive(true);*/

        localPlayer.GetComponent<Inventory>().Open();
        inv.Open();
        RefreshStateLabel();
        return true;
    }

    public override bool NeedShowLabel()
    {
        return !inv.IsOpen && PlayerThere;
    }

    public override void ExitFrom(GameObject player)
    {
        WasdDoing();
    }

    public override void WasdDoing()
    {
        if (inv.IsOpen)
        {
            localPlayer.GetComponent<Inventory>().Close();
            RefreshStateLabel();
        }
    }
}
