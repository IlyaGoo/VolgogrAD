using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell {

    public GameObject Object;
    public int Amount;
    public ItemData Content;
    public string ItemName = "";

    public Cell(GameObject gameObject) {
        Object = gameObject;
    }

    bool HasOwner => Content.owner != null;

    public bool CanInteract(GameObject mayBeOwner)
    {
        return Content == null || !HasOwner || Content.owner == mayBeOwner;
    }

    public void SetTarget(bool state, bool freeze = false, bool force = false)
    {
        Object.GetComponent<HoverCell>().SetTarget(state, freeze, force);
    }

    public void Clear()
    {
        Content = null;
        ItemName = "";
        Amount = 0;
        if (Object != null)
            Object.GetComponent<HoverCell>().Clear();
    }

    public void Put(ItemData item)
    {
        Put(item, 1);
       /* Content = item;
        nm = item.name;

        Amount++;
        GameObject img = Object.transform.Find("Image").gameObject;
        GameObject text = Object.transform.Find("Amount").gameObject;
        img.GetComponent<Image>().sprite = item.Icon;
        img.SetActive(true);
        text.GetComponent<Text>().text = Amount.ToString();
        if (Amount > 1) text.SetActive(true);
        if (Object.GetComponent<HoverCell>().isActive) Object.GetComponent<HoverCell>().NeedDoSomething();*/
    }

    public void SetAmount(int amount) {
        Amount = amount;
        GameObject text = Object.transform.Find("Amount").gameObject;
        text.transform.Find("Text").GetComponent<Text>().text = Amount.ToString();
        text.SetActive(Amount > 1);
        if (Object.GetComponent<HoverCell>().isActive) Object.GetComponent<HoverCell>().NeedDoSomething();
    }

    public void Put(ItemData item, int amount) { 
        if (item == null || item.name != ItemName && ItemName != "")
        {
            return;
        }
        Content = item;
        ItemName = item.name;
        Amount += amount;

        if (Object == null) return;

        UpdateHover(Content);
    }

    public void PutForce(ItemData item, int amount)
    {
        if (item == null)
        {
            return;
        }
        Content = item;
        ItemName = item.name;
        Amount = amount;

        if (Object == null) return;

        UpdateHover(Content);
    }

    public void UpdateHover(ItemData item)
    {
        GameObject img = Object.transform.Find("Image").gameObject;
        GameObject text = Object.transform.Find("Amount").gameObject;
        var currentHover = Object.GetComponent<HoverCell>();
        currentHover.ShowDescription();

        img.GetComponent<Image>().sprite = item.Icon;
        img.SetActive(true);
        text.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Amount.ToString();
        text.SetActive(Amount > 1);

        if (currentHover.isActive) currentHover.NeedDoSomething();

        if(currentHover.invObj.inventoryType == InventoryType.Panel)
            currentHover.invObj.choosenCell.ObjectAddedToPanel(ItemName);
    }

    public void RemoveAll() {
        Remove(Amount);
    }

    public void Remove(int count)
    {
        Amount -= count;
        var currentHover = Object.GetComponent<HoverCell>();
        MonoBehaviourExtension.localCommands.CmdPlaySound(1);
        if (Amount < 1)
        {
            
            var previousName = string.Copy(ItemName);

            Content = null;
            Amount = 0;
            ItemName = "";
            if (Object == null) return;
            currentHover.Clear();
            if (currentHover.isActive) currentHover.NeedDoSomething();
            else if (currentHover.invObj.inventoryType == InventoryType.Panel)
            {
                currentHover.invObj.choosenCell.ObjectRemovedFromPanel(previousName);
            }
        }
        else
            UpdateHover(Content);
    }
}