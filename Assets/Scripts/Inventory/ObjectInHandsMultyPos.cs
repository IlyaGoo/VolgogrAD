using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInHandsMultyPos : ObjectInHands
{
    public Vector3 sidePosition2 = new Vector3(-0.5f, 0, 0);
    public Vector3 backPosition2 = new Vector3(0, 0.6f, 0.6f);
    public Vector3 frontPosition2 = new Vector3(0, -0.2f, -0.2f);
    public bool SecondPos = false;

    public void ChangePos(bool state)
    {
        SecondPos = state;
        SetPositions();
    }

    protected override void SetPositions()
    {
        if (SecondPos)
        {
            backPos1 = new Vector3(backPosition2.x, backPosition2.y, backPosition2.z);
            backPos2 = new Vector3(-backPosition2.x, backPosition2.y, backPosition2.z);
            frontPos1 = new Vector3(frontPosition2.x, frontPosition2.y, frontPosition2.z);
            frontPos2 = new Vector3(-frontPosition2.x, frontPosition2.y, frontPosition2.z);
        }
        else
        {
            backPos1 = new Vector3(backPosition.x, backPosition.y, backPosition.z);
            backPos2 = new Vector3(-backPosition.x, backPosition.y, backPosition.z);
            frontPos1 = new Vector3(frontPosition.x, frontPosition.y, frontPosition.z);
            frontPos2 = new Vector3(-frontPosition.x, frontPosition.y, frontPosition.z);
        }
        positiveScale = new Vector3(startScale.x, startScale.y, startScale.z);
        negativeScale = new Vector3(-startScale.x, startScale.y, startScale.z);
        base.SetPosition(currentState, currentScale, true);
    }
}
