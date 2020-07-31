using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;
using System;

public class PlayerNet : NetworkBehaviour, IAltLabelShower
{
    public GameObject invPanelLeft;
    public GameObject invPanelRight;
    public Moving _moving;
    [SerializeField]
    Light cameraLight;
    [SerializeField]
    private GameObject _camera;
    [SerializeField]
    private Taker _taker;
    public Commands cmd;
    [SerializeField]
    private AudioSource _audioSource;
    private TaskManager _taskManager;
    public ListenersManager listenersManager;
    public HealthBar healthBar;
    public Skiper _skiper;
    public PlayerInventoryController inventoryController;
    [SerializeField] GameObject FListenerObject;
    public DescriptionController descriptionController;

    public PlayerMetaData _meta;

    public void ShowLabel(GameObject player) { }

    public TaskManager Task
    {
        get
        {
            if (_taskManager == null)
            {
                _taskManager = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
            }
            return _taskManager;
        }
    }

    Vector3 offset = Vector3.up;
    public Vector3 Offset { get => offset; }
    private GameObject _panelPr;
    public GameObject Panel 
    { 
        get 
        { 
            return _panelPr; 
        } 
        set 
        {
            if (_panelPr != null)
            {
                GetComponent<Moving>().dontBeReflect.Remove(_panelPr);
                Destroy(_panelPr);
            }
            _panelPr = value;
            if (_panelPr != null)
                GetComponent<Moving>().dontBeReflect.Add(_panelPr);
        }
    }
    public Transform TranformForPanel { get => transform; set => throw new System.NotImplementedException(); }
    public string LabelName { get => nickname; set => throw new System.NotImplementedException(); }

    public string nickname = "";
    [SerializeField]
    private GameObject uiPrefab;
    [SerializeField]
    private GameObject enviromentTrigger;
    [SerializeField]
    private GameObject music;
    [SerializeField]
    private GameObject envTriger;

    [SerializeField] private GameObject[] headPrefabs;
    [SerializeField] private GameObject[] bodyPrefabs;
    [SerializeField] private GameObject[] legsPrefabs;

    public string id;

    [SerializeField] private GameObject inventoryDataPrefab = null;
    public GameObject inventoryData = null;

    private GameObject canvas = null;

    Vector3 WholeWector1 = new Vector3(0.2f, 0.2f, 0);
    Vector3 WholeWector2 = new Vector3(0, -0.2f, 0);

    // Use this for initialization

    public GameObject InitInventory()
    {
        if (inventoryData == null)
        {
            inventoryData = Instantiate(inventoryDataPrefab);
            //GetComponent<Inventory>().data = inventoryData.GetComponent<InventoryData>();
            
/*            foreach (var inv in GetComponentsInChildren<Inventory>()) 
            { 
                if (inv.name == "MouseInventory")
                    inv.data = inventoryData.transform.GetChild(0).GetComponent<InventoryData>();
                else if (inv.name == "MainInventory")
                    inv.data = inventoryData.transform.GetChild(1).GetComponent<InventoryData>();
                else
                    inv.data = inventoryData.GetComponent<InventoryData>();
                inv.data.Init();
            }*/

            inventoryController.inventories[0].data = inventoryData.transform.GetChild(1).GetComponent<InventoryData>();
            inventoryController.inventories[0].data.Init();
            cmd.TargetForceRequestInventory(connectionToClient, gameObject, 0);
            inventoryController.inventories[1].data = inventoryData.GetComponent<InventoryData>();
            inventoryController.inventories[1].data.Init();
            cmd.TargetForceRequestInventory(connectionToClient, gameObject, 1);
            inventoryController.inventories[2].data = inventoryData.transform.GetChild(0).GetComponent<InventoryData>();
            inventoryController.inventories[2].data.Init();
            cmd.TargetForceRequestInventory(connectionToClient, gameObject, 2);
            //GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<Inventory>().data = inventoryData.GetComponent<InventoryData>();

        }
        return inventoryData;
    }

    public void AddDoing(string id, TriggerAreaDoing doing)
    {
        if (!isServer) return;
        Task.GetPlayerData(id).AddDoing(doing);
    }

    public void RemoveDoing(string id, TriggerAreaDoing doing)
    {
        if (!isServer) return;
        Task.GetPlayerData(id).RemoveDoing(doing);
    }

    [Command]
    public void CmdUnstuck()
    {
        RpcUnstuck();
    }

    [ClientRpc]
    public void RpcUnstuck()
    {
        _moving.SetAllRenders(false);
        _moving.stacked = true;
    }

    [Command]
    public void CmdDig(GameObject whole)
    {
        RpcDig(whole, whole.GetComponent<WholeScript>().currentLevel + 1);
    }

    [ClientRpc]
    public void RpcDig(GameObject whole, int toLevel)
    {
        whole.GetComponent<WholeScript>().Dig(toLevel);
    }

    public void SendDepth(GameObject whole)
    {
        TargetDig(connectionToClient, whole, whole.GetComponent<WholeScript>().currentLevel, whole.transform.position);
    }

    [TargetRpc]
    void TargetDig(NetworkConnection target, GameObject whole, int toLevel, Vector3 pos)
    {
        whole.GetComponent<WholeScript>().GetParent(pos);
        whole.GetComponent<WholeScript>().Dig(toLevel);
    }

    [Command]
    public void CmdPrintToServer(string str)
    {
        print(str);
    }


    [Command]
    public void CmdDigV(Vector3 whole)
    {
        RpcDigV(whole);
    }

    [ClientRpc]
    public void RpcDigV(Vector3 whole)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(whole + WholeWector1, WholeWector2);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("StandartBlock"))
            {
                var ren = hit.transform.GetComponent<SpriteRenderer>();
                var i = int.Parse(ren.sprite.name[ren.sprite.name.Length - 1].ToString());
                if (i != 3)
                {
                    ren.sprite = Resources.LoadAll<Sprite>("Ground2")[i + 1];
                }
                break;
            }
        }
    }


    [Command]
    public void CmdWakeUp()
    {
        RpcWakeUp();
    }

    [ClientRpc]
    public void RpcWakeUp()
    {
        _moving.SetAllRenders(true);
        _moving.stacked = false;
    }

    [Command]
    public void CmdSetWhole(Vector3 pos, string meta)
    {
        RpcSetWhole(pos, meta);
    }

    [ClientRpc]
    public void RpcSetWhole(Vector3 pos, string meta)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos + WholeWector1, WholeWector2);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("StandartBlock"))
            {
                if (hit.collider.gameObject.GetComponent<BlockController>() != null)
                    return;
                hit.collider.gameObject.AddComponent<BlockController>();
                if (isServer)
                {
                    SpawnWhole("WholeObject", hit.transform.position, hit.transform.rotation, meta);
                }
                break;
            }
        }
    }

    public void SendWhole(Vector3 pos)
    {
        TargetSetWhole(connectionToClient, pos);
    }

    [TargetRpc]
    void TargetSetWhole(NetworkConnection target, Vector3 pos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos + WholeWector1, WholeWector2);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("StandartBlock"))
            {
                if (hit.collider.gameObject.GetComponent<BlockController>() != null)
                    return;
                hit.collider.gameObject.AddComponent<BlockController>();
                break;
            }
        }
    }

    public void SpawnWhole(string prefab, Vector3 position, Quaternion rotation, string meta)
    {
        var wholeObject = Instantiate(Resources.Load(prefab), position, rotation) as GameObject;
        NetworkServer.Spawn(wholeObject);
        wholeObject.GetComponent<WholeScript>().SetData(meta);
        Task.wholes.Add(wholeObject);
        //RpcSetWholeParent(gameObject);
    }

/*    [ClientRpc]
    void RpcSetWholeParent(GameObject whole)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(whole.transform.position + new Vector3(0.1f, 0.1f, 0), Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("StandartBlock"))
            {
                whole.transform.parent = hit.transform;
                hit.transform.GetComponent<BlockController>().CurrentWhole = whole.GetComponent<WholeScript>();
                return;
            }
        }
    }*/

    [Command]
    private void CmdSpawnParts(int x, int y, int z)
    {
        RpcSpawnParts(x, y, z);
        Task.PlayersDatas[GetComponent<NetworkIdentity>().netId.ToString()].SetBody(x, y, z);
    }

    [Command]
    public void CmdSendNickname(string nick)
    {
        SendNickname(nick);
    }

    public void SendNickname(string nick)
    {
        RpcSendNickname(nick);
    }

    [ClientRpc]
    private void RpcSendNickname(string nick)
    {
        nickname = nick;
    }

    [ClientRpc]
    private void RpcSpawnParts(int newHeadNum, int newBodyNum, int newLagsNum)
    {
        _moving.SpawnParts(newHeadNum, newBodyNum, newLagsNum);
    }

    public void SendSpawnParts(GameObject obj)
    {
        var mov = obj.GetComponent<StandartMoving>();
        TargetSpawnParts(connectionToClient, obj, mov.headNum, mov.bodyNum, mov.lagsNum);
    }

    public void SendNickname(GameObject obj)
    {
        var plNet = obj.GetComponent<PlayerNet>();
        TargetSendNickname(connectionToClient, obj, plNet.nickname);
    }

    [TargetRpc]
    void TargetSendNickname(NetworkConnection target, GameObject obj, string nick)
    {
        obj.GetComponent<PlayerNet>().nickname = nick;
    }

    [TargetRpc]
    void TargetSpawnParts(NetworkConnection target, GameObject obj, int newHeadNum, int newBodyNum, int newLagsNum)
    {
        obj.GetComponent<StandartMoving>().SpawnParts(newHeadNum, newBodyNum, newLagsNum);
    }

    [Command]
    void CmdSendPlayerData(string identity, string name, GameObject playerObject)
    {
        Task.PlayerSendedData(identity, name, playerObject);
        id = identity;
    }

    public override void OnStartLocalPlayer()
    {
        _meta = GameObject.Find("PlayerMetaData").GetComponent<PlayerMetaData>();
        _taskManager = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
        _taskManager._taker = _taker;
        foreach (var helper in FindObjectsOfType<ChatPlayerHelper>())
            helper.Init(_meta, _taskManager);//Инициализируем ChatPlayerHelper
        _taskManager.Init(gameObject);//Инициализируем TaskManager


        //_taskManager.globalLight = cameraLight;
        /*var docPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/VolgogrAD";
        using (StreamReader sw = new StreamReader(docPath + "/player.txt", System.Text.Encoding.Default))
        {
            FindObjectOfType<ChatHelper>().Nickname = nickname = sw.ReadLine();
            CmdSendPlayerData(GetComponent<NetworkIdentity>().netId.ToString(), nickname, gameObject);
            var bodyNums = sw.ReadLine().Split(',');
            headNum = int.Parse(bodyNums[0]);
            bodyNum = int.Parse(bodyNums[1]);
            legsNum = int.Parse(bodyNums[2]);

        }*/
        _taskManager.chatHelper.Nickname = nickname = _meta.nickname;
        id = GetComponent<NetworkIdentity>().netId.ToString();
        CmdSendPlayerData(id, nickname, gameObject);
        _moving.headNum = _meta.headNum;
        _moving.bodyNum = _meta.bodyNum;
        _moving.lagsNum = _meta.legsNum;

        CmdSpawnParts(_moving.headNum, _moving.bodyNum, _moving.lagsNum);
        //CmdSendAnimation();
        CmdSendNickname(nickname);

        music.SetActive(true);
        gameObject.name = "LocalPlayer";
        envTriger.SetActive(true);
        _moving.enabled = _audioSource.enabled = true;
        _camera.SetActive(true);
        //GetComponent<KeyController>().enabled = true;
        inventoryController.EnableInventories();
        GetComponent<HealthBar>().enabled = true;
        GetComponent<AltListener>().enabled = true;

        _taskManager.optionsScript.Init();

        canvas = _taskManager.canvas;
        invPanelLeft = _taskManager.invPanelLeft;
        invPanelRight = _taskManager.invPanelRight;

        descriptionController = canvas.GetComponent<DescriptionController>();
        descriptionController.Player = gameObject;

        //GetComponent<KeyController>().Init();
        inventoryController.inventories[0].Open(gameObject, false);
        inventoryController.inventories[2].Open(gameObject, false);

/*        foreach (var inv in inventoryController.inventories)
        {
            if (inv.inventoryType == InventoryType.Panel || inv.inventoryType == InventoryType.EmptyInventory)
                inv.Open(false);
        }*/
        listenersManager.enabled = true;
        listenersManager.TabListeners.Add(GetComponent<Inventory>());
        
        var callMenu = canvas.GetComponent<CallMenu>();
        listenersManager.EscListeners.Add(callMenu);
        //GetComponentsInChildren<Inventory>().Init();

        invPanelLeft.SetActive(false);
        invPanelRight.SetActive(false);
        //GameObject.FindGameObjectWithTag("ItemDescription").SetActive(false);
        FListenerObject.SetActive(true);

        callMenu.enabled = true;
        callMenu.Player = gameObject;
        //Invoke("Request", 0.1f);
        if (!isServer)
            GetComponent<Connector>().RequestAll();
        _taskManager._mobManager.Init(gameObject, this);
        _taker.Init();
        GetComponentInChildren<CircleTrigger>().isController = true;
        _taskManager.menuSctipt.manager = _meta.netWorkManager;
        _taskManager.menuSctipt.manager.SetLoading(false);
    }

    void Request()
    {
        foreach (var inv in GetComponentsInChildren<Inventory>())
        {
            if (inv.inventoryType == InventoryType.MainInventory || inv.inventoryType == InventoryType.Panel)
            {
                inv.Player = gameObject;
                inv.RequesData();
            }
        }
    }

    void OnDestroy()
    {
        if (!isLocalPlayer && Task.isServer)
        {
            Task.GetPlayerData(id).EndAllDoins(id);
            Task.PlayerDisConnectedNickname(id);//Здесь же удаляем из datas
            Destroy(inventoryData);
        }
    }
}
