public class SetLightLevelOnce: AbstractOnceTimeDoing
{
    private float intensy;
    private float secondIntensy;
    public SetLightLevelOnce(int doTime, float intensy, float secondIntensy, bool onlyServer) : base(doTime, onlyServer)
    {
        this.intensy = intensy;
        this.secondIntensy = secondIntensy;
    }

    public override void Doing()
    {
        LightsController.instance.SetLight(intensy, secondIntensy);
    }
}
