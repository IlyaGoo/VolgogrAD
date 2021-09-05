using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioScript : MonoBehaviour
{
    public static RadioScript instance;
    public GameObject panelesParent;
    public OnceRadio[] paneles;
    public int currentRadioChoosenNum = -1;

    private RadioScript()
    { }

    private void Awake()
    {
        instance = this;
    }

    public void ChooseRadio(int num)
    {
        if (currentRadioChoosenNum == num) return;
        if (currentRadioChoosenNum != -1) paneles[currentRadioChoosenNum].SetState(false);
        currentRadioChoosenNum = num;
        paneles[currentRadioChoosenNum].SetState(true);
    }

}
