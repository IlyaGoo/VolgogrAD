using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverCell : MonoBehaviourExtension, IPointerEnterHandler, IPointerExitHandler, 
    IBeginDragHandler, IDragHandler, IDropHandler, IPointerClickHandler {

    [HideInInspector]
    private Cell cellRef;
    bool show = false;
    public Vector3 size = new Vector3(300, 50);
    public bool isActive;
    public Inventory invObj;

    public int y;
    public int x;

    [SerializeField] Sprite deactiveSprite;
    [SerializeField] Sprite activeSprite;

    public Cell CellRef { get => cellRef; set 
        { 
            cellRef = value;
            if (cellRef.ItemName == "")
                return;
            var itemData = ((GameObject)Resources.Load(cellRef.ItemName)).GetComponent<Item>().CopyItem();
            cellRef.UpdateHover(itemData);
        } }

    public void SetTarget(bool state, bool freeze, bool force = false)
    {
        if (state == isActive && !force) return;
        isActive = state;
        invObj.choosenCell = state ? this : null;
        GetComponent<Image>().sprite = state ? activeSprite : deactiveSprite;

        if (!freeze) {
            if (!state)
            {
                localMoving.inHandsNames.Clear();
                localCommands.CmdDestroyObjectInHands(localPlayer);
            }
            NeedDoSomething(); 
        }
    }

    public void NeedDoSomething()//сначала чекаем мб оно уже в руках
    {
        if (isActive)
        {
            if (CellRef.Content != null)
            {
                if (!localMoving.HasInHands(CellRef.Content.inHandsPrefab))
                {
                    DoSomething();
                }
                SpawnDopObjects();
            }
            else
            {
                DoSomething();
            }
        }
    }

    public void ObjectRemovedFromPanel(string previousName)
    {
        if (invObj != null && !invObj.freeze && CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null && CellRef.Content.canBeInHandsAlso.Contains(previousName) && invObj.HasIn(previousName) == null)
        {
            localCommands.CmdDestroyObjectInHands(localPlayer);
            localMoving.inHandsNames.Clear();
            invObj.choosenCell.NeedDoSomething();
        }
    }

    public void ObjectAddedToPanel(string obName)
    {
        if (invObj != null && !invObj.freeze && CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null && CellRef.Content.canBeInHandsAlso.Contains(obName))
            SpawnDopObjects();
    }

    public void SpawnDopObjects()
    {
        if (CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null)
        {
            bool spawned = false;
            foreach (var SecondItem in CellRef.Content.canBeInHandsAlso)
            {
                ItemData itData;
                if ((itData = invObj.HasIn(SecondItem)) != null)
                {
                    if (!localMoving.HasInHands(itData.inHandsPrefab))
                    {
                        spawned = true;
                        localCommands.CmdSpawnObjectInHands(localPlayer, itData.inHandsPrefab);
                    }
                }
            }

            if (spawned)
                localCommands.CmdChangeInHandsPosition(localPlayer, CellRef.Content.inHandsPrefab[1], true);
        }
    }

    void DoSomething()
    {
        localCommands.CmdDestroyObjectInHands(localPlayer); //Убираем из рук все вещи

        if (CellRef.Content != null && CellRef.Content.inHandsPrefab.Length != 0)
        {
            localCommands.CmdSpawnObjectInHands(localPlayer, CellRef.Content.inHandsPrefab);
        }
    }

    // вызывается, когда наводим курсор на ячейку
    public void OnPointerEnter(PointerEventData eventData) {
        show = true;
        if (CellRef.Content != null) {
            ShowDescription();
            if (CellRef.CanInteract(localPlayer))
                localFListener.currentHover = this;
        }
    }
    
    public void ShowDescription()
    {
        if (!show || invObj.inventoryType == InventoryType.EmptyInventory || CellRef.Content == null) return;
        descriptionController.ShowDescription(this);
    }

    // вызывается, когда когда убираем курсор с ячейки
    public void OnPointerExit(PointerEventData eventData) {
        if (show)
        {
            descriptionController.DestroyRef(gameObject);
            show = false;
        }
        if (localFListener.currentHover = this)
            localFListener.currentHover = null;
    }

    // вызывается, когда кликаем на ячейку и начинам тащить
    public void OnBeginDrag(PointerEventData eventData) {
        if (CellRef.CanInteract(localPlayer))
            IfClickOn();
    }

    // вызывается, когда отпускаем перетаскивание и находимся над этой ячейкой
    public void OnDrop(PointerEventData eventData)
    {
        if (!CellRef.CanInteract(localPlayer)) return;
        var mouseCell = localPlayerInventoryController.inventories[2].cells[0, 0];
        if (mouseCell.ItemName != "")
        {
            localCommands.CmdReplaceItem(localPlayer, invObj.InventoryControllerObject, 2, invObj.Number, mouseCell.ItemName, 0, 0, mouseCell.Amount, y, x);
        }

        return;
    }

    // вызывается, когда кликаем на эту ячейку
    public void OnPointerClick(PointerEventData eventData) {
        // если кликнули правой кнопкой
        if (!CellRef.CanInteract(localPlayer)) return;
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (CellRef.Content == null) return;
            if (CellRef.Content.Type == ItemCategory.Potion) {
                localCommands.CmdUseItem(invObj.InventoryControllerObject, invObj.Number, cellRef.ItemName, y, x, 1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            IfClickOn();
        }
    }

    void IfClickOn()
    {

        var mouseCell = localPlayerInventoryController.inventories[2].cells[0, 0];
        if (mouseCell.ItemName != "")
        {
            localCommands.CmdReplaceItem(localPlayer, invObj.InventoryControllerObject, 2, invObj.Number, mouseCell.ItemName, 0, 0, mouseCell.Amount, y, x);
        }
        else if(CellRef.ItemName != "")
        {
            var count = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? cellRef.Amount / 2 + cellRef.Amount % 2 : cellRef.Amount;
            localCommands.CmdReplaceItem(invObj.InventoryControllerObject, localPlayer, invObj.Number, 2, CellRef.Content.PrefabName, y, x, count, 0, 0);
        }
    }

    public void Clear()
    {
        transform.Find("Image").gameObject.SetActive(false);
        transform.Find("Amount").gameObject.SetActive(false);
        descriptionController.DestroyRef(gameObject);
    }

    void OnDestroy()
    {
        if (show)
            descriptionController.DestroyRef(gameObject);
    }

    public void OnDrag(PointerEventData eventData)//без этого не работает перетаскивание предметов
    {
    }
}