using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StandartMoving : NetworkBehaviour
{
    public float speed = 1.4f;
    public List<ObjectInHands> objectsInHands = new List<ObjectInHands>();
    public List<string> inHandsNames = new List<string>();
    public int frontState = -1; //сверху вних 1 2 3

    public Animator headAnimator;
    public Animator bodyAnimator;
    public Animator lagsAnimator;

    public bool vanished = false;

    public int headNum = -1;
    public int bodyNum = -1;
    public int lagsNum = -1;
    protected bool inited = false;
    bool bodySpawned = false;

    protected readonly float multiplayer = Mathf.Sin(45);
    protected float currentMultiplayer;
    protected float shiftMultiplayer = 1;

    protected bool inWater;
    public int waterLevel;
    public bool stacked = false;

    public bool isShifted = false;

    public List<GameObject> dontBeReflect = new List<GameObject>();

    public void SetAnim(float xMove, float yMove, float multyplayer, bool newShifter)
    {
        int newState = frontState;
        if (yMove == 1) newState = 1;
        else if (yMove == -1) newState = 3;
        else if (Mathf.Abs(xMove) == 1) newState = 2;
        if (frontState != newState)
        {
            frontState = newState;
            UpdateObjectsInHandsState();
        }
        SendAnims(xMove, yMove);

        if (currentMultiplayer != multyplayer)
        {
            currentMultiplayer = multyplayer;
            headAnimator.speed = bodyAnimator.speed = lagsAnimator.speed = multyplayer;
        }
        if (newShifter != isShifted)
        {
            isShifted = newShifter;
            UpdateObjectsInHands();
        }
    }

    public void SendAllData(int state, bool shifted)
    {
        if (state == -1) return;
        isShifted = shifted;
        frontState = state;
        var needState = "StayBack";
        switch (state) {
            case 3:
                needState = "Stay";
                break;
            case 2:
                needState = "StaySide";
                break;
        }
        headAnimator.Play(needState);
        bodyAnimator.Play(needState);
        lagsAnimator.Play(needState);
        UpdateObjectsInHandsState();
        UpdateObjectsInHands();
    }

    void SendAnims(float x, float y)
    {
        headAnimator.SetFloat("x_move", x);
        headAnimator.SetFloat("y_move", y);

        bodyAnimator.SetFloat("x_move", x);
        bodyAnimator.SetFloat("y_move", y);

        lagsAnimator.SetFloat("x_move", x);
        lagsAnimator.SetFloat("y_move", y);
    }

    public void SpawnParts(int newHeadNum, int newBodyNum, int newLagsNum)
    {
        if (bodySpawned) return;
        bodySpawned = true;

        headNum = newHeadNum;
        bodyNum = newBodyNum;
        lagsNum = newLagsNum;
        headAnimator.runtimeAnimatorController = Resources.Load("Animations/Head" + newHeadNum + "Animator") as AnimatorOverrideController;
        bodyAnimator.runtimeAnimatorController = Resources.Load("Animations/Body" + newBodyNum + "Animator") as AnimatorOverrideController;
        lagsAnimator.runtimeAnimatorController = Resources.Load("Animations/Lags" + newLagsNum + "Animator") as AnimatorOverrideController;
    }

    public void ChangeScale(float xScale)
    {
        transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        foreach (var refl in dontBeReflect)
        {
            refl.transform.localScale = new Vector3(xScale == 1 ? Math.Abs(refl.transform.localScale.x) : -Math.Abs(refl.transform.localScale.x), refl.transform.localScale.y, refl.transform.localScale.z);
        }
        UpdateObjectsInHandsState();
    }

    public void UpdateObjectsInHands()
    {
        if (isShifted)
            foreach (var ob in objectsInHands)
            {
                ob.gameObject.SetActive(true);
            }
        else
        {
            foreach (var ob in objectsInHands)
            {
                if (ob.CompareTag("FindCircle"))
                {
                    ob.GetComponent<MetallDetectorScript>().CloseAll();
                    ob.gameObject.SetActive(false);
                }
            }
        }
    }

    public void UpdateObjectsInHandsState()
    {
        var scaleBool = transform.localScale.x == 1;
        foreach (var ob in objectsInHands)
        {
            ob.SetPosition(frontState, scaleBool);
        }
    }

    public void UpdateObjectsInHandsValues()
    {
        var scaleBool = transform.localScale.x == 1;
        foreach (var ob in objectsInHands)
        {
            ob.SetPositionValues(frontState, scaleBool);
        }
    }

    public abstract PlayerNet PlNet();
    protected abstract bool DigIsLocal();
    protected abstract bool FindIsController();

    public void SetObjectInHandsO(string[] obj)
    {
        if (obj.Length == 0) return;

        if (!inWater) bodyAnimator.SetBool("inHands", true);
        foreach (var ob in obj)
        {
            inHandsNames.Add(ob);
            if (ob.Equals("DoingDelayScale") || inWater) continue; //TODO как-то по-другому это делать нужно офк
            var newObj = Instantiate(Resources.Load("InHands/" + ob), transform.position, Quaternion.identity) as GameObject;
            if (newObj.CompareTag("Dig"))
                newObj.GetComponent<Dig>().isLocal = isLocalPlayer;
            else if (newObj.CompareTag("FindCircle"))
            {
                newObj.GetComponent<MetallDetectorScript>().Init(PlNet(), FindIsController());
            }
            var obComp = newObj.GetComponent<ObjectInHands>();
            objectsInHands.Add(obComp);

            newObj.transform.parent = transform;
            obComp.CustomStart();
        }
        //UpdateObjectsInHandsValues();
        UpdateObjectsInHandsState();
        UpdateObjectsInHands();
    }

    public void DestroyOb()
    {
        foreach (var ob in objectsInHands)
        {
            Destroy(ob.gameObject);
        }
        objectsInHands.Clear();
        inHandsNames.Clear();
        bodyAnimator.SetBool("inHands", false);
    }

    public void SetAllRenders(bool state)
    {
        vanished = !state;
        lagsAnimator.GetComponent<SpriteRenderer>().enabled = state;
        bodyAnimator.GetComponent<SpriteRenderer>().enabled = state;
        headAnimator.GetComponent<SpriteRenderer>().enabled = state;
    }

    public virtual void EnterInWater(int deep)
    {
        waterLevel = deep;
        switch (deep)
        {
            case 0:
                inWater = false;
                /*lagsAnimator.gameObject.SetActive(true);
                bodyAnimator.gameObject.SetActive(true);*/
                lagsAnimator.GetComponent<SpriteRenderer>().enabled = true;
                bodyAnimator.GetComponent<SpriteRenderer>().enabled = true;
                break;
            case 1:
                inWater = true;
                shiftMultiplayer = 0.6f;
                /*lagsAnimator.gameObject.SetActive(false);
                bodyAnimator.gameObject.SetActive(true);*/
                lagsAnimator.GetComponent<SpriteRenderer>().enabled = false;
                bodyAnimator.GetComponent<SpriteRenderer>().enabled = true;
                break;
            case 2:
                inWater = true;
                shiftMultiplayer = 0.4f;
                /*lagsAnimator.gameObject.SetActive(false);
                bodyAnimator.gameObject.SetActive(false);*/
                lagsAnimator.GetComponent<SpriteRenderer>().enabled = false;
                bodyAnimator.GetComponent<SpriteRenderer>().enabled = false;
                break;
        }
    }

    public void DestroyScaleInHands()
    {
        foreach (var ob in objectsInHands)
        {
            if (ob.name.Substring(0, ob.name.Length - 7) == "DoingDelayScale")
            {
                var scaleScript = ob.GetComponent<DoingScaleScript>();
                scaleScript.Close();
                inHandsNames.Remove("DoingDelayScale");
                break;
            }
        }
        bodyAnimator.SetBool("inHands", objectsInHands.Count != 0);
    }

    public void SetScalesDestroyingBool()//Метод нужен, чтобы шкала не вызвала второй раз метод дестроинга шкалы в OnGUI
    {
        foreach (var ob in objectsInHands)
        {
            if (ob.name.Substring(0, ob.name.Length - 7) == "DoingDelayScale")
            {
                var scaleScript = ob.GetComponent<DoingScaleScript>();
                scaleScript.allradyDestroying = true;
                return;
            }
        }
    }

    public void SetScaleInHands(float time, int energy, bool isController, int soundNum, ScaleDoing bl = null)
    {
        if (inHandsNames.Contains("DoingDelayScale")) 
        {
            DestroyScaleInHands();
            //return;
            /*int i;
            for (i = 0; i < objectsInHands.Count; i++)
            {
                if (objectsInHands[i].CompareTag("DoingScale"))
                    break;
            }
            objectsInHands[i].GetComponent<DoingScaleScript>().Close();*/
        };
        var newObj = Instantiate(Resources.Load("InHands/DoingDelayScale"), transform) as GameObject;
        objectsInHands.Add(newObj.GetComponent<ObjectInHands>());
        inHandsNames.Add("DoingDelayScale");
        //newObj.transform.parent = transform;
        newObj.GetComponent<DoingScaleScript>().Init(gameObject, bl, energy, isController, soundNum, time);
        newObj.transform.parent = transform;
        newObj.GetComponent<ObjectInHands>().CustomStart();
        UpdateObjectsInHandsState();
        UpdateObjectsInHands();
    }

    public void RemoveObjectsInHands(string[] obj)
    {

        foreach (var ob in obj)
        {
            if (inHandsNames.Contains(ob))
                inHandsNames.Remove(ob);

            ObjectInHands deletedObj = null;// = objectsInHands.Add(newObj.GetComponent<ObjectInHands>());
            foreach (var obInstance in objectsInHands)
            {
                if (obInstance.name.Substring(0, obInstance.name.Length - 7) == ob)
                {
                    deletedObj = obInstance;
                    break;
                }
            }
            if (deletedObj != null)
            {
                objectsInHands.Remove(deletedObj);
                Destroy(deletedObj.gameObject);
            }
        }

        if (objectsInHands.Count == 0)
            bodyAnimator.SetBool("inHands", false);
    }

    public bool HasInHands(string[] names)
    {
        foreach (var obName in names)
        {
            bool contains = false;
            foreach (var nameInHands in inHandsNames)
            {
                if (nameInHands.Equals(obName))
                {
                    contains = true;
                    break;
                }
            }

            if (!contains)
            {
                return false;
            }
        }
        return true;
    }

    public void ChangeInHandsPosition(string name, bool state)
    {
        foreach (var ob in objectsInHands)
        {
            if (ob.name.Substring(0, ob.name.Length - 7) == name)
            {
                ob.GetComponent<ObjectInHandsMultyPos>().ChangePos(state);
                return;
            }
        }
    }
}
