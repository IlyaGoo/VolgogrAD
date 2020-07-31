using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FListener : MonoBehaviour, IListener
{
    [SerializeField] ListenersManager manager;
    [SerializeField] Commands cmd;
    [SerializeField] Moving mov;
    public HoverCell currentHover;
    public Inventory inventoryPanel;

    public void EventDid()
    {
        if (!mov.CanThrove)
        {
            mov.GetComponent<HeadMessegesManager>().AddMessege("Мб в другой раз");
            return;
        }

        if (currentHover != null)
        {
            int count = Input.GetKey(KeyCode.LeftControl) ? currentHover.CellRef.Amount : 1;
            cmd.CmdThroveItem(currentHover.invObj.InventoryCintrollerObject, currentHover.invObj.Number, currentHover.CellRef.ItemName, currentHover.y, currentHover.x, count, transform.position, Quaternion.identity, currentHover.CellRef.Content.owner);
        }
        else if (inventoryPanel.choosenCell != null && inventoryPanel.choosenCell.CellRef.ItemName != "")
        {
            int count = Input.GetKey(KeyCode.LeftControl) ? inventoryPanel.choosenCell.CellRef.Amount : 1;
            cmd.CmdThroveItem(inventoryPanel.choosenCell.invObj.InventoryCintrollerObject, inventoryPanel.choosenCell.invObj.Number, inventoryPanel.choosenCell.CellRef.ItemName, inventoryPanel.choosenCell.y, inventoryPanel.choosenCell.x, count, transform.position, Quaternion.identity, inventoryPanel.choosenCell.CellRef.Content.owner);

        }
    }

    void Start()
    {
        manager.FListeners.Add(this);
    }
}
