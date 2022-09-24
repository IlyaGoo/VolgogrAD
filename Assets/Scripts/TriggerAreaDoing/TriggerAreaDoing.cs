using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class TriggerAreaDoing : MonoBehaviourExtension, ICanBeOwn
{
    static readonly string triggerTextPrefix = "Texts/TriggerText";
    public string labelText = null;
    protected GameObject LabelObject;
    public int labelSize = 0;
    public bool PlayerThere;
    /** Существует ли реакция области на движение WASD игрока */
    public bool HasWASDReqction;
    public bool needPushButton = true;
    public Vector3 textOffset = new Vector3(-0.6f, 1, 0);

    public GameObject owner;
    public virtual GameObject Owner { get => owner; set => owner = value; }

    public abstract bool Do();

    public virtual bool NeedShowLabel()
    {
        return true;
    }

    public virtual void RefreshStateLabel()
    {
        TurnLabel(NeedShowLabel());
    }

    protected virtual Transform GetPanelParent() => transform;

    public virtual void WasdDoing()
    { 
    }

    /**
     * Выполняется только на сервере
     */
    public virtual void DisconnectExit(string id)
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
            LabelObject = Instantiate((GameObject)Resources.Load(triggerTextPrefix + labelSize), transform.position + textOffset, Quaternion.identity, GetPanelParent());
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

    public virtual bool CanInteract(GameObject interactEntity)
    {
        return true;
    }
}
