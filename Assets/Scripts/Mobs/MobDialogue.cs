using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobDialogue : MonoBehaviour
{
    public List<MobController> stayingMobs = new List<MobController>();
    public bool Empty => stayingMobs.Count == 0;
    DialogueData currentDialogueData;
    int botIndex = 0;
    float delayTimer = 0;

    public void AddStayMob(MobController mob)
    {
        stayingMobs.Add(mob);
        if (stayingMobs.Count == 2)
            StartDialogue();
        else if (stayingMobs.Count > 2)
        {
            //var waitingDoing = mob.currentDoings[0] as BotWaiting;
            (mob.currentDoings[0] as BotWaiting).SetTimer(currentDialogueData.NeedTime - currentDialogueData.indexOfPosition * currentDialogueData.messegesDelay);
            //waitingDoing.needTime = Math.Max(waitingDoing.needTime, currentDialogueData.NeedTime - currentDialogueData.indexOfPosition * currentDialogueData.messegesDelay);
        }
    }

    void StartDialogue()
    {
        currentDialogueData = new DialogueData()
        {
            messages = new BotFraze[] { 
                new BotFraze(){text = "Привет"},
                new BotFraze(){text = "Привет"}
            }
        };

        foreach (var mob in stayingMobs)
        {
            (mob.currentDoings[0] as BotWaiting).SetTimer(currentDialogueData.NeedTime);
            //waitingDoing.needTime = Math.Max(waitingDoing.needTime, currentDialogueData.NeedTime);
        }

        ThrowFirstMessage();
    }

    void ThrowFirstMessage()
    {
        stayingMobs[0].GetComponent<HeadMessagesManager>().AddMessege(currentDialogueData.CurrentText);
    }

    private void Update()
    {
        if (currentDialogueData == null) return;
        delayTimer += Time.deltaTime;
        if (delayTimer >= currentDialogueData.messegesDelay)
        {
            delayTimer = 0;
            currentDialogueData.indexOfPosition++;
            if (currentDialogueData.CurrentFraze.needChangeSpeaker)
                botIndex = (botIndex + 1) % stayingMobs.Count;
            stayingMobs[botIndex].GetComponent<HeadMessagesManager>().AddMessege(currentDialogueData.CurrentText);
            if (currentDialogueData.IsEnd) currentDialogueData = null;
        }
    }

    public void RemoveStayMob(MobController mob)
    {
        stayingMobs.Remove(mob);
        if (stayingMobs.Count < 2) currentDialogueData = null;
    }
}

public class DialogueData
{
    public float NeedTime => messages.Length * messegesDelay;
    public float messegesDelay = 2;
    public int indexOfPosition = 0;
    public BotFraze[] messages;

    public BotFraze CurrentFraze => messages[indexOfPosition];
    public string CurrentText => messages[indexOfPosition].text;
    public bool IsEnd => indexOfPosition + 1 == messages.Length;
}

public class BotFraze
{
    public bool needChangeSpeaker = true;
    public string text;
}