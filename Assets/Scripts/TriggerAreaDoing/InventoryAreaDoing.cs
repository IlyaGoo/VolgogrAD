using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAreaDoing : TriggerAreaDoing
{
    [SerializeField] Inventory inv;

    public override bool Do(GameObject player)
    {
/*        var cont = player.GetComponent<PlayerNet>();
        cont.invPanelLeft.SetActive(true);
        cont.invPanelRight.SetActive(true);*/

        player.GetComponent<Inventory>().Open(player);
        inv.Open(player);
        TurnLabel(NeedShowLabel());
        return true;
    }

    public override bool NeedShowLabel()
    {
        return !inv.IsOpen && PlayerThere;
    }

    public override void ExitFrom(GameObject player)
    {
        WasdDoing(player);
    }

    public override void WasdDoing(GameObject player)
    {
        if (inv.IsOpen)
        {
            player.GetComponent<Inventory>().Close();
            TurnLabel(NeedShowLabel());
        }
    }
}
