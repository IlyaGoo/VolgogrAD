using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrovePlace : MonoBehaviour, IDropHandler, IPointerClickHandler
{

    GameObject player;
    GameObject Player { get { if (player == null) player = GameObject.Find("LocalPlayer");
            return player; } }

    Inventory inv;
    Inventory Inv { get { 
            if (inv == null) inv = Player.GetComponent<PlayerInventoryController>().inventories[2];
            return inv; } }

    Moving mov;
    Moving Mov
    {
        get
        {
            if (mov == null) mov = Player.GetComponent<Moving>();
            return mov;
        }
    }

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
            if (!Mov.CanThrove)
            {
                Player.GetComponent<HeadMessegesManager>().AddMessege("Мб в другой раз");
                //TODO кинуть сообщение не могу кинуть
                return;
            }
            var pos = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.y / 100);

            Player.GetComponent<Commands>().CmdThroveItem(Player, 2, mouseCell.Content.PrefabName, 0, 0, mouseCell.Amount, pos, Quaternion.identity, mouseCell.Content.owner);


            //pl.GetComponent<Commands>().CmdReplaceItem(pl, invObj.InventoryCintrollerObject, 2, invObj.Number, mouseCell.nm, 0, 0, mouseCell.Amount, y, x);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Drop();
    }

}
