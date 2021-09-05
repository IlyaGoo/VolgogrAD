/**По сути потом весь этот класс будет внутри xml**/
public class PointData
{
    public int[] neeedPointsNums;
    public string name;
    public int minigameControllerNum;
    public int needCount;

    public PointData(string name, int minigameControllerNum, int needCount, int[] newNeedPointsNums = null)
    {
        this.name = name;
        this.minigameControllerNum = minigameControllerNum;
        this.neeedPointsNums = newNeedPointsNums;
        this.needCount = needCount;
        neeedPointsNums = newNeedPointsNums;
    }
}