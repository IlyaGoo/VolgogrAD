using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : MonoBehaviour
{
    int plCount = 0;
    [SerializeField] SpriteRenderer[] renders;

    public void RemovePlayer()
    {
        plCount--;
        if (plCount == 0)
            SetOff();
    }

    public void AddPlayer()
    {
        plCount++;
        if (plCount == 1)
            SetOn();

    }
    
    void SetOn()
    {
        SetTransparency(0.5f);
        SetRendersState(true);
    }

    void SetOff()
    {
        SetTransparency(1);
        SetRendersState(false);
    }

    void SetRendersState(bool state)
    {
        foreach (var render in renders)
            render.enabled = state;
    }

    void SetTransparency(float transparencyValue)
    {
        var objectRender = GetComponent<SpriteRenderer>();
        objectRender.color = new Color(objectRender.color.r, objectRender.color.g, objectRender.color.b, transparencyValue);
    }
}
