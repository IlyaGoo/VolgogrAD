using UnityEngine;

public class BotInfo: AbstractEntityInfo
{
    public override int GetBodyNum => EntityObject.GetComponent<StandartMoving>().bodyNum;
    public override int GetHeadNum => EntityObject.GetComponent<StandartMoving>().headNum;
    public override int GetLegsNum => EntityObject.GetComponent<StandartMoving>().lagsNum;
    
    public override bool IsDisconnected => false;

    public BotInfo(GameObject obj, string id)
        :base(obj, id)
    {
    }
}
