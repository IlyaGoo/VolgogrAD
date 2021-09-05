public class SendBotsToSleepTimeDoing : AbstractOnceTimeDoing
{
    public SendBotsToSleepTimeDoing(int doTime, bool onlyServer) : base(doTime, onlyServer)
    {
    }

    public override void Doing()
    {
        MobsManager.instance.SendAllBotsToSleep();
    }
}
