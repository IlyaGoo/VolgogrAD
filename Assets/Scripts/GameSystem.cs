using System;
using Mirror;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    /** Переменная, отвечающая за то, что все инициализировано
     * Были вынуждены ее использовать из-за того, что FixedUpdate вызывался слишком рано
     */
    public bool allInited;
    public GameObject localPlayer;
    public PlayerNet localPlayerNet;
    public Commands localCommands;
    public Moving localMoving;
    public Taker localTaker;
    public FListener localFListener;
    public HealthBar localHealthBar;
    public ListenersManager localListenersManager;
    public ChatPlayerHelper localChatPlayerHelper;
    public InventoryController localPlayerInventoryController;
    public HeadMessagesManager localHeadMessagesManager;
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
    protected static GameObject localPlayer => GameSystem.instance.localPlayer;
    public static TaskManager taskManager => TaskManager.instance;
    protected static PlayerNet localPlayerNet => GameSystem.instance.localPlayerNet;
    public static Commands localCommands => GameSystem.instance.localCommands;
    protected static InventoryController localPlayerInventoryController => GameSystem.instance.localPlayerInventoryController;
    protected static ChatPlayerHelper localChatPlayerHelper => GameSystem.instance.localChatPlayerHelper;
    protected string localPlayerId => GameSystem.instance.localPlayerId;
    protected static PlayerMetaData playerMetaData => PlayerMetaData.instance;
    protected static Moving localMoving => GameSystem.instance.localMoving;
    protected static HeadMessagesManager localHeadMessagesManager => GameSystem.instance.localHeadMessagesManager;
    protected static ListenersManager localListenersManager => GameSystem.instance.localListenersManager;
    protected static HealthBar localHealthBar => GameSystem.instance.localHealthBar;
    protected static DebafsController debafsController => DebafsController.instance;
    protected static ChatHelper chatHelper => ChatHelper.instance;
    protected static CallMenu callMenu => CallMenu.instance;
    protected static RadioScript radioScript => RadioScript.instance;
    public static ObjectsScript objectsScript => ObjectsScript.instance;
    protected static Taker localTaker => GameSystem.instance.localTaker;
    protected static DescriptionController descriptionController => DescriptionController.instance;
    protected static MobsManager mobsManager => MobsManager.instance;
    protected static Algo algo => Algo.instance;
    protected static FListener localFListener => GameSystem.instance.localFListener;
    protected static TaskMenuScript taskMenu => TaskMenuScript.instance;
}

public class NetworkBehaviourExtension : NetworkBehaviour
{
    protected static GameObject localPlayer => GameSystem.instance.localPlayer;
    public static TaskManager taskManager => TaskManager.instance;
    protected static PlayerNet localPlayerNet => GameSystem.instance.localPlayerNet;
    public static Commands localCommands => GameSystem.instance.localCommands;
    protected static InventoryController localPlayerInventoryController => GameSystem.instance.localPlayerInventoryController;
    public static ChatPlayerHelper localChatPlayerHelper => GameSystem.instance.localChatPlayerHelper;
    protected string localPlayerId => GameSystem.instance.localPlayerId;
    protected static PlayerMetaData playerMetaData => PlayerMetaData.instance;
    protected static Moving localMoving => GameSystem.instance.localMoving;
    protected static HeadMessagesManager localHeadMessagesManager => GameSystem.instance.localHeadMessagesManager;
    protected static ListenersManager localListenersManager => GameSystem.instance.localListenersManager;
    protected static HealthBar localHealthBar => GameSystem.instance.localHealthBar;
    protected static DebafsController debafsController => DebafsController.instance;
    protected static ChatHelper chatHelper => ChatHelper.instance;
    protected static CallMenu callMenu => CallMenu.instance;
    protected static RadioScript radioScript => RadioScript.instance;
    public static ObjectsScript objectsScript => ObjectsScript.instance;
    protected static Taker localTaker => GameSystem.instance.localTaker;
    protected static DescriptionController descriptionController => DescriptionController.instance;
    protected static MobsManager mobsManager => MobsManager.instance;
    protected static Algo algo => Algo.instance;
    protected static FListener localFListener => GameSystem.instance.localFListener;
    protected static TaskMenuScript taskMenu => TaskMenuScript.instance;
}

public class WTFException : Exception
{
    public WTFException(string message): base(message)
    {

    }
    /*
     * Эксепшон для случаев, когда хз как вообще можно было так сделать э
     * (Да, на самом деле чисто ради названия лол)
     */
}
