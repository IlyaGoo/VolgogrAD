/**Действие, которое должно выполняться каждую секунду игрового времени в определенном промежутке**/
public abstract class AbstractTimeDoing : TimeDoing
{
    private bool onlyServer;

    public AbstractTimeDoing(int startTime, int endTime, bool onlyServer)
        : base(startTime, endTime)
    {
        this.onlyServer = onlyServer;
    }
        
    public virtual void Doing(){}

    public void Do()
    {
        if(!onlyServer || GameSystem.instance.localPlayerNet.isServer)
            Doing();
    }
}