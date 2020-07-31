using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioScript : MonoBehaviour
{
    public GameObject panelesParent;
    public OnceRadio[] paneles;
    public int currentRadioChoosenNum = -1;

    public void ChooseRadio(int num)
    {
        if (currentRadioChoosenNum == num) return;
        if (currentRadioChoosenNum != -1) paneles[currentRadioChoosenNum].SetState(false);
        currentRadioChoosenNum = num;
        paneles[currentRadioChoosenNum].SetState(true);
    }

}
