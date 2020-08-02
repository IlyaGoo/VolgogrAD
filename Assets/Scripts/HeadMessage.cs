using Boo.Lang.Environments;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeadMessege : MonoBehaviour
{
    float needTime = 4;
    float currentTimer = 4;
    HeadMessegesManager manager;
    [SerializeField] TextMeshPro textMesh;

    public void SetText(string text, HeadMessegesManager currentManager)
    {
        manager = currentManager;
        textMesh.text = text;
    }

    void Update()
    {
        needTime -= Time.deltaTime;
        if (needTime <= 0)
            Close();
    }

    public void Close(bool needRemove = true)
    {
        manager.mov.dontBeReflect.Remove(gameObject);
        if (needRemove)
            manager.massages.Remove(this);
        Destroy(gameObject);
    }
}
