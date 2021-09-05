using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Debaf : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FormerlySerializedAs("nm")] public string debafName;
    public string description;
    public GameObject player;
    [SerializeField] BafType bafType;
    [SerializeField] float needTime;
    [SerializeField] float tickNeedTime = -1;
    public float currentTime;
    float currentTickTime;
    public bool haveTimeEnd;
    public int num;
    public bool needShow = true;
    DebafsController controller;
    [SerializeField] GameObject icon;
    [SerializeField] Animator currentAnimator;

    public static float delta;
    public static Vector3 size = new Vector3(300, 50);

    void Update()
    {
        if (tickNeedTime != -1)
        {
            currentTickTime += Time.deltaTime;
            if (currentTickTime >= tickNeedTime)
            {
                Do();
                currentTickTime = 0;
            }
        }
        if (!haveTimeEnd)
            return;
        currentTime += Time.deltaTime;
        if (needTime - currentTime <= 5)
        {
            currentAnimator.SetInteger("state", 1);
        }
        if (currentTime >= needTime)
            Off();
    }

    public void Reinit()
    {
        currentAnimator.SetInteger("state", 0);
        currentTime = 0;
    }

    public void ShowDescription()
    {
        controller.descController.ShowDescription(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
        if (currentAnimator.GetInteger("state") == 0)
            currentAnimator.SetInteger("state", 2);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyRef();
        if (currentAnimator.GetInteger("state") == 2)
            currentAnimator.SetInteger("state", 0);
    }

    void DestroyRef()
    {
        controller.descController.DestroyRef(gameObject);
    }

    public void On(GameObject pl, float time, DebafsController ctrl ,bool ce = true)
    {
        player = pl;
        controller = ctrl;
        haveTimeEnd = ce;
        if (time != -1)
            needTime = time;
        Do();
    }

    public virtual void Do()
    {
        
    }

    public void Off()
    {
        DestroyRef();
        controller.RemoveDebaf(this);
        Destroy(gameObject);
    }
}
