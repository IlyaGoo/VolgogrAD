using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggingController : TaskControllerScript
{
    Vector3 pointLeftBot;
    Vector3 pointRightTop;

    private void Awake()
    {
        var deltaY = -_minigame[0].size.y / 2 + _minigame[0].offset.y;
        var deltaX = -_minigame[0].size.x / 2 + _minigame[0].offset.x;
        var y1 = transform.position.y + deltaY;
        var y2 = transform.position.y - deltaY;
        pointLeftBot = new Vector3(transform.position.x + deltaX, y1, y1);
        pointRightTop = new Vector3(transform.position.x - deltaX, y2, y2);
    }

    public (Vector3, Vector3) GetPoints()
    {
        return (pointLeftBot, pointRightTop);
    }
}
