using Boo.Lang.Environments;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeadMessage : MonoBehaviour
{
    float needTime = 4;
    float currentTimer = 0;
    HeadMessagesManager currentManager;
    [SerializeField] TextMeshPro textMesh;

    public void SetText(string text, HeadMessagesManager newManager)
    {
        currentManager = newManager;
        textMesh.text = text;
    }

    void Update()
    {
        currentTimer += Time.deltaTime;
        if (currentTimer >= needTime)
            Close();
    }

    public void Close(bool needRemove = true)
    {
        currentManager.mov.dontBeReflect.Remove(gameObject);
        if (needRemove)
            currentManager.massages.Remove(this);
        Destroy(gameObject);
    }
}
