using UnityEngine;

public class SetLightObjectsTimeDoing: AbstractOnceTimeDoing
{
    private bool needState;
    public SetLightObjectsTimeDoing(int doTime, bool needState, bool onlyServer) : base(doTime, onlyServer)
    {
        this.needState = needState;
    }

    public override void Doing()
    {
        TaskManager.instance.SetNightObject(needState);
    }
}