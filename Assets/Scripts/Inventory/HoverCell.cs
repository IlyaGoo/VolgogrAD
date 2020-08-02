using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
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

        var mov = invObj.Player.GetComponent<Moving>();

        if (!freeze) { 
            if (!state)
            {
                mov.inHandsNames.Clear();
                mov.plNet.cmd.CmdDestroyObjectInHands(mov.gameObject);
            }
            NeedDoSomething(); 
        }
    }

    public void NeedDoSomething()//сначала чекаем мб оно уже в руках
    {
        if (isActive)
        {
            var mov = invObj.Player.GetComponent<Moving>();
            if (CellRef.Content != null)
            {
                if (!mov.HasInHands(CellRef.Content.inHandsPrefab))
                {
                    DoSomething(mov);
                }
                SpawnDopObjects(mov);
            }
            else
            {
                DoSomething(mov);
            }
        }
    }

    public void ObjectRemovedFromPanel(string previousName)
    {
        if (invObj != null && !invObj.freeze && CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null && CellRef.Content.canBeInHandsAlso.Contains(previousName) && invObj.HasIn(previousName) == null)
        {
            invObj.mov.plNet.cmd.CmdDestroyObjectInHands(invObj.mov.gameObject);
            invObj.mov.inHandsNames.Clear();
            invObj.choosenCell.NeedDoSomething();
        }
    }

    public void ObjectAddedToPanel(string obName)
    {
        if (invObj != null && !invObj.freeze && CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null && CellRef.Content.canBeInHandsAlso.Contains(obName))
            SpawnDopObjects(invObj.Player.GetComponent<Moving>());
    }

    public void SpawnDopObjects(Moving mov)
    {
        if (CellRef.Content != null && CellRef.Content.canBeInHandsAlso != null)
        {
            bool spawned = false;
            foreach (var SecondItem in CellRef.Content.canBeInHandsAlso)
            {
                ItemData itData;
                if ((itData = invObj.HasIn(SecondItem)) != null)
                {
                    if (!mov.HasInHands(itData.inHandsPrefab))
                    {
                        spawned = true;
                        mov.plNet.cmd.CmdSpawnObjectInHands(mov.gameObject, itData.inHandsPrefab);
                    }
                }
            }

            if (spawned)
                mov.plNet.cmd.CmdChangeInHandsPosition(mov.gameObject, CellRef.Content.inHandsPrefab[1], true);
        }
    }

    void DoSomething(Moving mov)
    {
        mov.plNet.cmd.CmdDestroyObjectInHands(mov.gameObject); //Убираем из рук все вещи

        if (CellRef.Content != null && CellRef.Content.inHandsPrefab.Length != 0)
        {
            mov.plNet.cmd.CmdSpawnObjectInHands(mov.gameObject, CellRef.Content.inHandsPrefab);
        }
    }

    // вызывается, когда наводим курсор на ячейку
    public void OnPointerEnter(PointerEventData eventData) {
        show = true;
        if (CellRef.Content != null) {
            ShowDescription();
            if (CellRef.CanInteract(invObj.Player))
                invObj.Player.transform.Find("FListener").GetComponent<FListener>().currentHover = this;
        }
    }
    
    public void ShowDescription()
    {
        if (!show || invObj.inventoryType == InventoryType.EmptyInventory || CellRef.Content == null) return;
        invObj.descController.ShowDescription(this);
    }

    // вызывается, когда когда убираем курсор с ячейки
    public void OnPointerExit(PointerEventData eventData) {
        if (show)
        {
            invObj.descController.DestroyRef(gameObject);
            show = false;
        }
        var l = invObj.Player.transform.Find("FListener").GetComponent<FListener>();
        if (l.currentHover = this)
            l.currentHover = null;
    }

    // вызывается, когда кликаем на ячейку и начинам тащить
    public void OnBeginDrag(PointerEventData eventData) {
        if (CellRef.CanInteract(invObj.Player))
            IfClickOn();
    }

    // вызывается, когда отпускаем перетаскивание и находимся над этой ячейкой
    public void OnDrop(PointerEventData eventData)
    {
        if (!CellRef.CanInteract(invObj.Player)) return;
        var mouseCell = invObj.Player.GetComponent<InventoryController>().inventories[2].cells[0, 0];
        if (mouseCell.ItemName != "")
        {
            invObj.Player.GetComponent<Commands>().CmdReplaceItem(invObj.Player, invObj.InventoryCintrollerObject, 2, invObj.Number, mouseCell.ItemName, 0, 0, mouseCell.Amount, y, x);
        }

        return;
    }

    // вызывается, когда кликаем на эту ячейку
    public void OnPointerClick(PointerEventData eventData) {
        // если кликнули правой кнопкой
        if (!CellRef.CanInteract(invObj.Player)) return;
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (CellRef.Content == null) return;
            if (CellRef.Content.Type == Category.Potion) {
                invObj.Player.GetComponent<Commands>().CmdUseItem(invObj.InventoryCintrollerObject, invObj.Number, cellRef.ItemName, y, x, 1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            IfClickOn();
        }
    }

    void IfClickOn()
    {

        var mouseCell = invObj.Player.GetComponent<InventoryController>().inventories[2].cells[0, 0];
        if (mouseCell.ItemName != "")
        {
            invObj.Player.GetComponent<Commands>().CmdReplaceItem(invObj.Player, invObj.InventoryCintrollerObject, 2, invObj.Number, mouseCell.ItemName, 0, 0, mouseCell.Amount, y, x);
        }
        else if(CellRef.ItemName != "")
        {
            var count = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? cellRef.Amount / 2 + cellRef.Amount % 2 : cellRef.Amount;
            invObj.Player.GetComponent<Commands>().CmdReplaceItem(invObj.InventoryCintrollerObject, invObj.Player, invObj.Number, 2, CellRef.Content.PrefabName, y, x, count, 0, 0);
        }
    }

    public void Clear()
    {
        transform.Find("Image").gameObject.SetActive(false);
        transform.Find("Amount").gameObject.SetActive(false);
        invObj.descController.DestroyRef(gameObject);
    }

    void OnDestroy()
    {
        if (show)
            invObj.descController.DestroyRef(gameObject);
    }

    public void OnDrag(PointerEventData eventData)//без этого не работает перетаскивание предметов
    {
    }
}