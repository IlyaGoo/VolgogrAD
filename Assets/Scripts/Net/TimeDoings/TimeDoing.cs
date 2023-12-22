/** Что-то, что привязано к определенномму промежутку времени, например, таски или исчезнование мотыльков */
public abstract class TimeDoing
{
    public int startTime;
    public int endTime;

    public TimeDoing(int startTime, int endTime)
    {
        this.startTime = startTime;
        this.endTime = endTime;
    }
}