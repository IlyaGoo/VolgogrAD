/**Устанавливает интенсивность света объектов после получночи, постепенно уменьшая свещение**/
public class SetLightLevelAfterMidnightTimeDoing: AbstractTimeDoing
{
    public SetLightLevelAfterMidnightTimeDoing(int startTime, int endTime, bool onlyServer) : base(startTime, endTime, onlyServer)
    {
    }

    public override void Doing()
    {
        var gameTimer = TaskManager.instance.gameTimer;
        TaskManager.instance.SetLight((gameTimer - 220) / 200f * 0.8f, 1 - (gameTimer - 220) / 200f);
    }
}
