using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo: AbstractEntityInfo{
    public string nickname;
    public List<TriggerAreaDoing> currentDoings = new List<TriggerAreaDoing>();
    public GameObject inventoryData;
    private int headNum;
    private int bodyNum;
    private int legsNum;

    public override int GetBodyNum => bodyNum;
    public override int GetHeadNum => headNum;
    public override int GetLegsNum => legsNum;
    public override bool IsDisconnected { get; set; } = false;

    public PlayerInfo(string nick, GameObject obj, string id)
        :base(obj, id)
    {
        nickname = nick;
        inventoryData = obj.GetComponent<PlayerNet>().InitInventory();
    }
    
    public void SetBody(int head, int body, int legs)
    {
        headNum = head;
        bodyNum = body;
        legsNum = legs;
    }

    public void EndAllDoins(string id)
    {
        foreach (var doing in currentDoings)
            doing.DisconnectExit(id);
    }

    public void AddDoing(TriggerAreaDoing newDoing)
    {
        currentDoings.Add(newDoing);
    }

    public void RemoveDoing(TriggerAreaDoing newDoing)
    {
        currentDoings.Remove(newDoing);
    }
}