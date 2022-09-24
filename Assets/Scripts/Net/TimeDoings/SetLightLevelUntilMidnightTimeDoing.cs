/**Устанавливает интенсивность света объектов до получночи, постепенно увеличивая свещение**/
public class SetLightLevelUntilMidnightTimeDoing: AbstractTimeDoing
{
    public SetLightLevelUntilMidnightTimeDoing(int startTime, int endTime, bool onlyServer) : base(startTime, endTime, onlyServer)
    {
    }

    public override void Doing()
    {
        var gameTimer = Timer.instance.gameTimer;
        LightsController.instance.SetLight(0.8f - (gameTimer - 1230) / 211f * 0.8f, (gameTimer - 1230) / 211f);
    }
}
