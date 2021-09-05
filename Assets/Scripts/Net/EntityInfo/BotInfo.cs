using UnityEngine;

public class BotInfo: AbstractEntityInfo
{
    public override int GetBodyNum => entityObject.GetComponent<StandartMoving>().bodyNum;
    public override int GetHeadNum => entityObject.GetComponent<StandartMoving>().headNum;
    public override int GetLegsNum => entityObject.GetComponent<StandartMoving>().lagsNum;
    
    public override bool IsDisconnected => false;

    public BotInfo(GameObject obj, string id)
        :base(obj, id)
    {
    }
}
