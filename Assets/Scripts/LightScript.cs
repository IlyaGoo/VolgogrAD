using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightScript : MonoBehaviour {

    [SerializeField] float maxIntecive = 0;
    [SerializeField] float minIntecive = 0;
    Light2D ownLight = null;
    [SerializeField] TaskManager man = null;
    public bool needUpdate = true;

    void Start()
    {
        Init();
        if (man == null) man = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<TaskManager>();
        man.AddLightoObject(this);
    }

    private void Init()
    {
        ownLight = GetComponent<Light2D>();
    }

    public void SetLight(float intence)
    {
        if(ownLight != null && needUpdate) ownLight.intensity = minIntecive + (maxIntecive - minIntecive) * intence;
    }

    public void SetFullLight()
    {
        if (ownLight != null) ownLight.intensity = man.currentLightLevel;
    }

    public void SetZeroLight()
    {
        if (ownLight != null) Init();
        ownLight.intensity = 0;
    }

    void OnDestroy()
    {
        var A = GameObject.FindGameObjectWithTag("TaskManager");
        var B = A == null ? null : A.GetComponent<TaskManager>();
        if (B != null) B.RemoveLightoObject(this);
    }

    void OnEnable()
    {
        if (man == null) return;
        SetLight(man.currentLightLevel);
    }
}
