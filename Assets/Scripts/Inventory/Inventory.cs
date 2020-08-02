using Mirror;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IListener, INumListener, IScrollListener
{

    public int ROWS = 4;
    public int COLS = 5;

    public GameObject Reference = null;
    public Transform parent = null;
    [SerializeField] Sprite panelSprite = null;
    public Cell[,] cells;
    public HoverCell choosenCell = null;
    [SerializeField]
    bool fromItemsPanel = true;
    public InventoryType inventoryType;
    [SerializeField] Sprite hoverSprite = null;

    static Vector3 posLeft = new Vector3(-260, 0, 0);
    static Vector3 posCenter = new Vector3(0, 0, 0);

    public Vector2 panelSize = new Vector2(550, 500);

    int currentCol = 0;
    int currentRaw = 0;
    public bool freeze = false;
    public InventoryData data;
    public GameObject InventoryCintrollerObject = null;
    public bool puttable = true;
    public int Number { get => System.Array.IndexOf(InventoryCintrollerObject.GetComponent<InventoryController>().inventories, this); }
    public bool IsOpen { get { if (cells != null) return cells[0, 0].Object != null; else return false; } }

    public DescriptionController descController = null;


    GameObject _player;
    ListenersManager lManaher;
    public PlayerNet playNet;
    public Moving mov;
    public GameObject Player { get 
        {
            if (_player == null)
            {
                _player = GameObject.Find("LocalPlayer");
                lManaher = _player.GetComponent<ListenersManager>();
                playNet = _player.GetComponent<PlayerNet>();
                mov = _player.GetComponent<Moving>();
            }
            return _player;
                }
        set
        {
            if (_player != null) return;
            _player = value;
            lManaher = _player.GetComponent<ListenersManager>();
            playNet = _player.GetComponent<PlayerNet>();
            mov = _player.GetComponent<Moving>();
        }
    }

    public ItemData HasIn(string name)
    {
        foreach (var cell in cells)
        {
            if (cell.ItemName == name) return cell.Content;
        }
        return null;
    }

    public void BackInHands()
    {
        if (cells == null || inventoryType != InventoryType.Panel) return;
        cells[currentRaw, currentCol].SetTarget(true, freeze, true);
    }

    public void SetFreeze(bool fr)
    {
        freeze = fr;
    }

    public void EventScrollDid(float move)
    {
        if (Mathf.Abs(move) < 0.2) return;
        cells[0, currentCol].SetTarget(false, freeze);

        if (move > 0)
        {
            currentCol = currentCol == COLS - 1 ? 0 : currentCol + 1;
        }
        else
        {
            currentCol = currentCol == 0 ? COLS - 1 : currentCol - 1;
        }

        cells[0, currentCol].SetTarget(true, freeze);
    }

    public void EventNumDid(int code)
    {
        if (currentCol == code) return;
        cells[0, currentCol].SetTarget(false, freeze);
        currentCol = code;
        cells[0, currentCol].SetTarget(true, freeze);
    }

    public void InitCells()
    {
        if (cells == null)
        {
            cells = new Cell[ROWS, COLS];
            for (int y = 0; y < ROWS; y++)
                for (int x = 0; x < COLS; x++)
                    cells[y, x] = new Cell(null);
        }
    }

    public void RequesData()
    {
        if (cells == null)
        {
            cells = new Cell[ROWS, COLS];
            for (int y = 0; y < ROWS; y++)
                for (int x = 0; x < COLS; x++)
                    cells[y, x] = new Cell(null);
        }
        else
            Clear();

        Player.GetComponent<Commands>().CmdRequestInventory(InventoryCintrollerObject, Number);
    }

    public void Clear()
    {
        for (int y = 0; y < ROWS; y++)
            for (int x = 0; x < COLS; x++)
                cells[y, x].Clear();
    }

    public void Open (GameObject pl, bool needRequest = true, bool isLeftPos = true) {
        if (cells != null && cells[0, 0].Object != null)
            return;
        Player = pl;
        switch (inventoryType)
        {
            case InventoryType.MainInventory:
                {
                    if (needRequest) RequesData();
                    lManaher.EscListeners.Add(this);//добавляем, что можем закрыться по esc
                    var currentPanel = playNet.invPanelLeft;
                    if (panelSprite != null)
                        currentPanel.GetComponent<Image>().sprite = panelSprite;
                    currentPanel.SetActive(true);
                    currentPanel.transform.localPosition = isLeftPos ? posLeft : posCenter;
                    parent = currentPanel.transform.Find("ItemsPanel");
                    parent.GetComponent<GridLayoutGroup>().constraintCount = COLS;
                    Reference = Resources.Load("Inventory/MainPanelCell") as GameObject;
                    break;
                }
            case InventoryType.Panel:
                if (needRequest) RequesData();
                parent = GameObject.FindGameObjectWithTag("ToolsPanel").transform;
                Reference = Resources.Load("Inventory/ToolsPanelCell") as GameObject;
                break;
            case InventoryType.EmptyInventory:
                parent = GameObject.FindGameObjectWithTag("MouseInventoryPanel").transform;
                Reference = Resources.Load("Inventory/EmptyCell") as GameObject;
                break;
            case InventoryType.Standart:
                {
                    if (needRequest) RequesData();
                    var panel = playNet.invPanelRight;
                    if (panelSprite != null)
                        panel.GetComponent<Image>().sprite = panelSprite;
                    panel.SetActive(true);
                    panel.GetComponent<RectTransform>().sizeDelta = panelSize;
                    parent = panel.transform.Find("ItemsPanel");
                    parent.GetComponent<GridLayoutGroup>().constraintCount = COLS;
                    Reference = Resources.Load("Inventory/MainPanelCell") as GameObject;
                    break;
                }
        }

        descController = playNet.descriptionController;

        bool needInit = false;
        if (cells == null)
        {
            cells = new Cell[ROWS, COLS];
            needInit = true;
        }

        for (int y = 0; y < ROWS; y++)
        {
            for (int x = 0; x < COLS; x++)
            {
                GameObject go = Instantiate(Reference, parent);
                if (needInit)
                    cells[y, x] = new Cell(go);
                else
                    cells[y, x].Object = go;
                var hov = go.GetComponent<HoverCell>();
                if (hoverSprite != null) go.GetComponent<Image>().sprite = hoverSprite;
                hov.CellRef = cells[y, x];
                hov.y = y;
                hov.x = x;
                hov.invObj = this;
            }
        }
        if (inventoryType == InventoryType.Panel) 
        { 
            cells[0, 0].SetTarget(true, freeze);
            var listenerManager = Player.GetComponent<ListenersManager>();
            listenerManager.AlphaListeners.Add(this);
            listenerManager.ScrollListeners.Add(this);
        }
    }

    public void Close()
    {
        for (int y = 0; y < ROWS; y++)
        {
            for (int x = 0; x < COLS; x++)
            {
                Destroy(cells[y, x].Object);
                cells[y, x].Object = null;
            }
        }
        switch (inventoryType)
        {
            case InventoryType.MainInventory:
                playNet.invPanelLeft.SetActive(false);
                lManaher.EscListeners.Remove(this);
                if (playNet.invPanelRight.activeSelf)
                    playNet.invPanelRight.transform.GetChild(0).GetChild(0).GetComponent<HoverCell>().invObj.Close();
                break;
            case InventoryType.Standart:
                playNet.invPanelRight.SetActive(false);
                break;
        }
    }

    public bool PutItem(ItemData item) {
        // ищем не полный стак для данного предмета
        if (AddItem(item)) return true;

        // если подобных предметов нет, или они переполнены, ищем свободное место
        if (AddNewItem(item)) return true;
        return false;
    }

    public bool AddItem(ItemData item)
    {
        for (int i = 0; i < ROWS; i++)
        {
            for (int j = 0; j < COLS; j++)
            {
                if (cells[i, j].Content == null) continue;
                if (cells[i, j].Content.Title == item.Title && cells[i, j].Amount + 1 <= item.MaxAmount)
                {
                    cells[i, j].Put(item);
                    return true;
                }
            }
        }
        return false;
    }

    public bool CanAddItem(ItemData item)
    {
        for (int i = 0; i < ROWS; i++)
        {
            for (int j = 0; j < COLS; j++)
            {
                if (cells[i, j].Content == null) return true;
                if (cells[i, j].Content.Title == item.Title && cells[i, j].Amount + 1 <= item.MaxAmount)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool AddNewItem(ItemData item)
    {
        for (int i = 0; i < ROWS; i++)
        {
            for (int j = 0; j < COLS; j++)
            {
                if (cells[i, j].Amount == 0)
                {
                    cells[i, j].Put(item);
                    return true;
                }
            }
        }
        return false;
    }

    public void EventDid()
    {
        if (IsOpen)
        {
            Close();
        }
        else
            Open(Player, true, false);
    }
}

public enum InventoryType
{
    Panel,
    MainInventory,
    EmptyInventory,
    Standart
}