using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightScript : MonoBehaviour {

    [SerializeField] float maxIntecive = 0;
    [SerializeField] float minIntecive = 0;
    Light2D OwnLight => GetComponent<Light2D>();
    [SerializeField] TaskManager man = null;
    public bool needUpdate = true;

    void Start()
    {
        if (man == null) man = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
        man.AddLightoObject(this);
    }

    public void SetLight(float intence)
    {
        if(OwnLight != null && needUpdate) OwnLight.intensity = minIntecive + (maxIntecive - minIntecive) * intence;
    }

    public void SetFullLight()
    {
        if (OwnLight != null) OwnLight.intensity = man.currentLightLevel;
    }

    public void SetZeroLight()
    {
        OwnLight.intensity = 0;
    }

    void OnDestroy()
    {
        var A = GameObject.FindGameObjectWithTag("TaskManager");
        var B = A == null ? null : A.GetComponent<TaskManager>();
        if (B != null) B.RemoveLightObject(this);
    }

    void OnEnable()
    {
        if (man == null) return;
        SetLight(man.currentLightLevel);
    }
}
