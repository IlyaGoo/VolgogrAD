/**Действие, которое должно выполниться 1 раз в определенное время**/
public abstract class AbstractOnceTimeDoing : AbstractTimeDoing
{
    public AbstractOnceTimeDoing(int doTime, bool onlyServer)
        :base(doTime, doTime, onlyServer)
    {}
}