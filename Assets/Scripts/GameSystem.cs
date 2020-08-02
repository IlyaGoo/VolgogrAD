using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public GameObject localPlayer;
    public PlayerNet playerNet;
    public Commands commands;
    public ChatPlayerHelper chatPlayerHelper;
    public InventoryController playerInventoryController;
    public string localPlayerId;

    public static GameSystem instance;

    private GameSystem()
    { }

    private void Awake()
    {
        instance = this;
    }
}

public class MonoBehaviourExtension : MonoBehaviour{
    protected GameObject localPlayer => GameSystem.instance.localPlayer;
    protected TaskManager taskManager => TaskManager.instance;
    protected PlayerNet playerNet => GameSystem.instance.playerNet;
    protected Commands commands => GameSystem.instance.commands;
    protected InventoryController playerInventoryController => GameSystem.instance.playerInventoryController;
    protected ChatPlayerHelper chatPlayerHelper => GameSystem.instance.chatPlayerHelper;
    protected string localPlayerId => GameSystem.instance.localPlayerId;
    public PlayerMetaData playerMetaData => PlayerMetaData.instance;
}

public class NetworkBehaviourExtension : NetworkBehaviour
{
    protected GameObject localPlayer => GameSystem.instance.localPlayer;
    protected TaskManager taskManager => TaskManager.instance;
    protected PlayerNet playerNet => GameSystem.instance.playerNet;
    protected Commands commands => GameSystem.instance.commands;
    protected InventoryController playerInventoryController => GameSystem.instance.playerInventoryController;
    protected ChatPlayerHelper chatPlayerHelper => GameSystem.instance.chatPlayerHelper;
    protected string localPlayerId => GameSystem.instance.localPlayerId;
    public PlayerMetaData playerMetaData => PlayerMetaData.instance;
}
