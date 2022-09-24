using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightScript : MonoBehaviourExtension
{
    public static readonly List<LightScript> allLights = new List<LightScript>();

    [SerializeField] float maxIntecive = 0;
    [SerializeField] float minIntecive = 0;
    UnityEngine.Rendering.Universal.Light2D OwnLight => GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    public bool needUpdate = true;

    private void Awake()
    {
        allLights.Add(this);
    }

    private void Start()
    {
        SetLight(LightsController.instance.currentLightLevel);
    }

    public void SetLight(float intence)
    {
        if(OwnLight != null && needUpdate) OwnLight.intensity = minIntecive + (maxIntecive - minIntecive) * intence;
    }

    public void SetFullLight()
    {
        if (OwnLight != null) OwnLight.intensity = LightsController.instance.currentLightLevel;
    }

    public void SetZeroLight()
    {
        OwnLight.intensity = 0;
    }

    void OnDestroy()
    {
        allLights.Remove(this);
    }

    void OnEnable()
    {
        if (LightsController.instance != null)//Такое происходит при первом заходе в сцену
            SetLight(LightsController.instance.currentLightLevel);
    }
}
