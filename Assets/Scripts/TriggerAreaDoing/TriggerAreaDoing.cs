using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class TriggerAreaDoing : MonoBehaviour {

    [SerializeField] string labelText = null;
    protected GameObject LabelObject;
    public bool PlayerThere;
    public bool WasdBool;
    public bool needPushButton = true;
    Vector3 textOffset = new Vector3(-0.6f, 1, 0);

    public abstract bool Do(GameObject player);

    public virtual bool NeedShowLabel()
    {
        return true;
    }

    protected virtual Transform GetPanelParent() => transform;

    public virtual void WasdDoing(GameObject player)
    { 
    }

    public virtual void DisconectExit(string id)
    {
    }

    public virtual void ExitFrom(GameObject id)
    {
    }

    public void ChangeText(string text)
    {
        labelText = text;
    }

    public void TurnLabel(bool state)
    {
        if (labelText == null) return;
        if (state && NeedShowLabel())
        {
            if (LabelObject != null) return;
            LabelObject = Instantiate((GameObject)Resources.Load("TriggerText"), transform.position + textOffset, Quaternion.identity, GetPanelParent());
            UpdateTextLabel();
        }
        else
            Destroy(LabelObject);
    }

    public void UpdateTextLabel()
    {
        if (LabelObject == null) return;
        LabelObject.GetComponentInChildren<TextMeshPro>().text = labelText;
    }

    private void OnDisable()
    {
        TurnLabel(false);
    }
}
