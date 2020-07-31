using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioSounder : NetworkBehaviour, IListener, IUpListener
{
    [SerializeField] RadioScript _radio = null;
    public OnceRadio currentRadio = null;
    public int currentRadioNum = -1;
    [SerializeField] AudioSource audioS;
    [SerializeField] AudioSource changeSource;
    public bool isOn = false;

    RadioScript Radio
    {
        get
        {
            if (_radio == null)
                _radio = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Radio").GetComponent<RadioScript>();
            return _radio;
        }
    }

    Commands _cmd;

    public Commands Cmd
    {
        get
        {
            if (_cmd == null)
            {
                _cmd = GameObject.Find("LocalPlayer").GetComponent<Commands>();
            }
            return _cmd;
        }
    }

    bool radioPanelActive = false;

    void ChangeRadio(int num)
    {
        Radio.ChooseRadio(num);
        if (currentRadio == Radio.paneles[num])
            return;
        //currentRadioNum = num;
        
        /*if (currentRadio != null)
        {
            currentRadio.SetState(false);
        }*/
        //currentRadio = null;
        /*Radio.paneles[num].SetState(true);*/
        Cmd.CmdChangeRadio(gameObject, num);
    }

    public void TakeMusic(int num, float delay)
    {
        SetMusic(currentRadio.GetMusic(num), delay);
    }

    public void ChangeRadioEverywere(int num)
    {
        changeSource.Play();
        if (isServer && currentRadio != null)
            currentRadio.RemoveSounder(this);
        currentRadioNum = num;
        currentRadio = Radio.paneles[num];
        if (isServer)
            currentRadio.AddSounder(this);
    }

    void Update()
    {
        if (radioPanelActive)
        {
            var xMath = Math.Abs(Input.GetAxis("Mouse X"));
            var yMath = Math.Abs(Input.GetAxis("Mouse Y"));
            if (xMath > 0.15f || yMath > 0.15f)
            {
                if (xMath > yMath)
                {
                    if (Input.GetAxis("Mouse X") > 0)
                    {
                        ChangeRadio(1);
                    }
                    else
                    {
                        ChangeRadio(3);
                    }
                }
                else
                {
                    if (Input.GetAxis("Mouse Y") > 0)
                    {
                        ChangeRadio(0);
                    }
                    else
                    {
                        ChangeRadio(2);
                    }
                }
            }

            float mw = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(mw) < 0.1) return;
            if (mw < 0.1)
            {
                ChangeRadio(Radio.currentRadioChoosenNum == 3 ? 1 : Radio.currentRadioChoosenNum + 1);
            }
            else if (mw > -0.1)
            {
                ChangeRadio(Radio.currentRadioChoosenNum == 0 ? 3 : Radio.currentRadioChoosenNum - 1);
            }
        }
    }

    public void EventDid()
    {
        radioPanelActive = true;
        Radio.panelesParent.SetActive(true);
    }

    public void EventUpDid()
    {
        radioPanelActive = false;
        Radio.panelesParent.SetActive(false);
    }
    
    public void SetMusic(AudioClip music, float startTime)
    {
        audioS.Stop();
        audioS.clip = music;
        if (music == null)
            return;
        audioS.time = startTime;
        audioS.Play();
    }

    public void ChooseRandom()
    {
        ChangeRadio(UnityEngine.Random.Range(0, Radio.paneles.Length));
    }

    public void Init()
    {
        if (currentRadioNum == -1)
        {
            ChooseRandom();
        }
        else //иначе сделать радио визуально выбранным в Q-меню
        {
            ChangeRadio(currentRadioNum);
        }
    }

    public void Set(bool state)
    {
        if (state == isOn) return;
        audioS.mute = !state;
        isOn = state;

        //if (!state && currentRadio != null) currentRadio.SetState(false);
        if (isServer && currentRadio != null)
        {
            if (state)
                currentRadio.AddSounder(this);
            else
                currentRadio.RemoveSounder(this);
        }
        currentRadio = null;
    }

}
