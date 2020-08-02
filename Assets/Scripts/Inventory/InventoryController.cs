using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour {

    public Inventory[] inventories;
	
	public bool CanPutItem(ItemData item)
    {
        foreach(var inv in inventories)
        {
            if (inv.puttable && inv.CanAddItem(item))
                return true;
        }
        return false;
    }

    public void EnableInventories()
    {
        foreach (var inv in inventories)
            inv.enabled = true;
    }
}
