using System.Collections.Generic;
using UnityEngine;

public class NightObject : MonoBehaviour
{
    public static readonly List<NightObject> allNightObjects = new List<NightObject>();
    
    private void Awake()
    {
        allNightObjects.Add(this);
    }

    public void SetState(bool state)
    {
        gameObject.SetActive(state);
    }
}
