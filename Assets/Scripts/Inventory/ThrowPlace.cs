using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowPlace : MonoBehaviourExtension, IDropHandler, IPointerClickHandler
{
    Inventory inv;
    Inventory Inv { get { 
            if (inv == null) inv = localPlayerInventoryController.inventories[2];
            return inv; } }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Drop();
        }
    }

    void Drop()
    {

        var mouseCell = Inv.cells[0, 0];
        if (mouseCell.ItemName != "")
        {
            if (!localMoving.CanThrove)
            {
                localHeadMessagesManager.AddMessege("Мб в другой раз");
                return;
            }
            var pos = new Vector3(localPlayer.transform.position.x, localPlayer.transform.position.y, localPlayer.transform.position.y / 100);

            localCommands.CmdThroveItem(localPlayer, 2, mouseCell.Content.PrefabName, 0, 0, mouseCell.Amount, pos, Quaternion.identity, mouseCell.Content.owner);
            //pl.GetComponent<Commands>().CmdReplaceItem(pl, invObj.InventoryCintrollerObject, 2, invObj.Number, mouseCell.nm, 0, 0, mouseCell.Amount, y, x);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Drop();
    }

}
