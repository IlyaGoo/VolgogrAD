using System.Collections.Generic;
using UnityEngine;

/**
 * Сложилась всратая ситуация, которую невозможно исправить (на самом деле пиздец как долго)
 * 
 * В случаях, когда игрок отключается как уебок и просто закрывает игру,
 * его персонаж просто удаляется, хотя перед этим мы должны сделать некоторые действия
 *
 * Для приемлемого решения этой проблемы был создан этот контроллер, который генерирует классы-информаторы,
 * хранящие в себе данные, которые нам необходимы после отключения игрока
 *
 * Тот же подход используется для мобов, чтобы унифицировать подход к ним и игрокам и оперировать
 * исключительно этими оболачками инофрмацией, не обращая внимание на объект сущности
 */
public class EntitysController : MonoBehaviourExtension
{
    public static EntitysController instance;
    private void Awake()
    {
        instance = this;
    }
    
    public readonly Dictionary<string, PlayerInfo> playersData = new Dictionary<string, PlayerInfo>();//netId Data
    public readonly Dictionary<string, BotInfo> botsData = new Dictionary<string, BotInfo>();//netId Data
    
    public PlayerInfo GetPlayerData(string id)
    {
        if (id == null)
            return null;
        return playersData.ContainsKey(id) ? playersData[id] : null;
    }
    
    public AbstractEntityInfo GetPlayerOrBotData(string id)
    {
        if (id == null)
            return null;
        if (playersData.ContainsKey(id))
            return playersData[id];
        if (botsData.ContainsKey(id))
            return botsData[id];
        return null;
    }

    private static bool IsServer => TaskManager.instance.isServer;
    
    public void CreatePlayerData(string identity, string nickname, GameObject playerObject)
    {
        if (playersData.ContainsKey(identity))
        {
            Debug.LogWarning("Попытка положить PlayerData на игрока, для которого она уже есть");
            return;
        }
        playersData.Add(identity, new PlayerInfo(nickname, playerObject, identity));
        if (IsServer) GameSystem.instance.localChatPlayerHelper.RpcSendLine(nickname + " присоединился к игре");
    }

    public void CreateBotData(string identity, GameObject playerObject)
    {
        if (botsData.ContainsKey(identity)) return;
        botsData.Add(identity, new BotInfo(playerObject, identity));
    }

    public void PlayerDisConnectedNickname(string identity)
    {
        if (!playersData.ContainsKey(identity)) return;
        if (IsServer) GameSystem.instance.localChatPlayerHelper.RpcSendLine(playersData[identity].nickname + " вышел из игры");
        playersData.Remove(identity);
    }
}
