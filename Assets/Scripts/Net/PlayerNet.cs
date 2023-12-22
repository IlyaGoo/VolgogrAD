using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Telepathy;

public class PlayerNet : NetworkBehaviourExtension, IAltLabelShower
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
    public ListenersManager listenersManager;
    public HealthBar healthBar;
    public Skiper _skiper;
    public InventoryController inventoryController;
    [SerializeField] GameObject FListenerObject;

    public void ShowLabel(GameObject player) { }

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
                _moving.dontBeReflect.Remove(_panelPr);
                Destroy(_panelPr);
            }
            _panelPr = value;
            if (_panelPr != null)
                _moving.dontBeReflect.Add(_panelPr);
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
            cmd.ForceRequestInventory(connectionToClient, gameObject, 0);
            inventoryController.inventories[1].data = inventoryData.GetComponent<InventoryData>();
            inventoryController.inventories[1].data.Init();
            cmd.ForceRequestInventory(connectionToClient, gameObject, 1);
            inventoryController.inventories[2].data = inventoryData.transform.GetChild(0).GetComponent<InventoryData>();
            inventoryController.inventories[2].data.Init();
            cmd.ForceRequestInventory(connectionToClient, gameObject, 2);
            //GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<Inventory>().data = inventoryData.GetComponent<InventoryData>();

        }
        return inventoryData;
    }

    public void AddDoing(string id, TriggerAreaDoing doing)
    {
        if (!isServer) return;
        EntitysController.instance.GetPlayerData(id).AddDoing(doing);
    }

    public void RemoveDoing(string id, TriggerAreaDoing doing)
    {
        if (!isServer) return;
        EntitysController.instance.GetPlayerData(id).RemoveDoing(doing);
    }

    [Command]
    public void CmdUnstuck()
    {
        RpcStuck();
    }

    [ClientRpc]
    public void RpcStuck()
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
        taskManager.wholes.Add(wholeObject);
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
        EntitysController.instance.GetPlayerData(GetComponent<NetworkIdentity>().netId.ToString()).SetBody(newHeadNum, newBodyNum, newLagsNum);
        _moving.SpawnParts(newHeadNum, newBodyNum, newLagsNum);
    }
    
    public void SendPlayerData(PlayerInfo playerInfo)
    {
        TargetSendPlayerData(connectionToClient, playerInfo.Identity, playerInfo.nickname, playerInfo.EntityObject);
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
    void CmdSendPlayerData(string identity, string playerNickname, GameObject playerObject)
    {
        RpcSendPlayerData(identity, playerNickname, playerObject);
    }
    
    [ClientRpc]
    private void RpcSendPlayerData(string identity, string playerNickname, GameObject playerObject)
    {
        EntitysController.instance.CreatePlayerData(identity, playerNickname, playerObject);
        id = identity;
    }
    
    [TargetRpc]
    void TargetSendPlayerData(NetworkConnection target, string identity, string playerNickname, GameObject playerObject)
    {
        EntitysController.instance.CreatePlayerData(identity, playerNickname, playerObject);
        id = identity;
    }

    void SetGameSystem()
    {
        GameSystem.instance.localPlayer = gameObject;
        GameSystem.instance.localPlayerNet = this;
        GameSystem.instance.localCommands = cmd;
        GameSystem.instance.localChatPlayerHelper = GetComponent<ChatPlayerHelper>();
        GameSystem.instance.localPlayerInventoryController = GetComponent<InventoryController>();
        GameSystem.instance.localPlayerId = GetComponent<NetworkIdentity>().netId.ToString();
        GameSystem.instance.localMoving = _moving;
        GameSystem.instance.localHeadMessagesManager = GetComponent<HeadMessagesManager>();
        GameSystem.instance.localListenersManager = listenersManager;
        GameSystem.instance.localHealthBar = healthBar;
        GameSystem.instance.localTaker = _taker;
        GameSystem.instance.localFListener = FListenerObject.GetComponent<FListener>();
    }

    public override void OnStartLocalPlayer()
    {
        SetGameSystem();
        
        taskManager.Init();//Инициализируем TaskManager
        
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
        chatHelper.Nickname = nickname = playerMetaData.nickname;
        id = localPlayerId;
        CmdSendPlayerData(localPlayerId, nickname, gameObject);
        _moving.headNum = playerMetaData.headNum;
        _moving.bodyNum = playerMetaData.bodyNum;
        _moving.lagsNum = playerMetaData.legsNum;

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
        healthBar.enabled = true;
        GetComponent<AltListener>().enabled = true;

        taskManager.optionsScript.Init();

        invPanelLeft = taskManager.invPanelLeft;
        invPanelRight = taskManager.invPanelRight;

        //GetComponent<KeyController>().Init();
        inventoryController.inventories[0].Open(false);
        inventoryController.inventories[2].Open(false);

/*        foreach (var inv in inventoryController.inventories)
        {
            if (inv.inventoryType == InventoryType.Panel || inv.inventoryType == InventoryType.EmptyInventory)
                inv.Open(false);
        }*/
        listenersManager.enabled = true;
        listenersManager.TabListeners.Add(GetComponent<Inventory>());
        
        listenersManager.EscListeners.Add(callMenu);
        //GetComponentsInChildren<Inventory>().Init();

        invPanelLeft.SetActive(false);
        invPanelRight.SetActive(false);
        //GameObject.FindGameObjectWithTag("ItemDescription").SetActive(false);
        FListenerObject.SetActive(true);

        callMenu.enabled = true;
        //Invoke("Request", 0.1f);
        if (!isServer)
            GetComponent<Connector>().CmdRequestAll();
        mobsManager.Init();
        _taker.Init();
        GetComponentInChildren<CircleTrigger>().isController = true;
        callMenu.manager = playerMetaData.netWorkManager;
        callMenu.manager.SetLoading(false);
        GameSystem.instance.allInited = true;
    }

    void Request()
    {
        foreach (var inv in GetComponentsInChildren<Inventory>())
        {
            if (inv.inventoryType == InventoryType.MainInventory || inv.inventoryType == InventoryType.Panel)
            {
                inv.RequesData();
            }
        }
    }

    void OnDestroy()
    {
        if (!isLocalPlayer && taskManager.isServer)
        {
            var playerInfo = EntitysController.instance.GetPlayerData(id);
            playerInfo.IsDisconnected = true;
            playerInfo.EndAllDoins(id);
            EntitysController.instance.PlayerDisConnectedNickname(id);//Здесь же удаляем из datas
            Destroy(inventoryData);
        }
    }
}
