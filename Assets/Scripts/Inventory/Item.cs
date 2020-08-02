using UnityEngine;
using System;
using TMPro;

public class Item : MonoBehaviourExtension, ICloneable, IAltLabelShower, ICanBeOwn
{

    public string Title;
    public string[] canBeInHandsAlso;
    [Multiline(5)]
    public string Description;
    public int MaxAmount;
    private int currentAmount = 1;
    public Category Type;
    public Sprite Icon;
    public string PrefabName;
    public string[] inHandsPrefab;
    public GameObject owner;
    public GameObject Owner { get => owner; set => owner = value; }

    Vector3 offset = Vector3.down;

    Color colorStandart = new Color(1, 1, 1, 1);
    Color colorRed = new Color(1, 0, 0, 1);

    public Vector3 Offset { get { 
            if (offset.y == -1) offset = new Vector3(GetComponent<BoxCollider2D>().size.x / 2, GetComponent<BoxCollider2D>().size.y, 0);
            return offset; } }

    private GameObject _panelPr;
    public GameObject Panel
    {
        get
        {
            return _panelPr;
        }
        set
        {
            if (_panelPr != null)
            {
                Destroy(_panelPr);
            }
            _panelPr = value;
        }
    }
    public Transform TranformForPanel { get => transform; set => throw new System.NotImplementedException(); }
    public string LabelName { get => Title + (CurrentAmount == 1 ? "" : " ("+ CurrentAmount+")"); set => throw new System.NotImplementedException(); }
    public int CurrentAmount { get => currentAmount; set {
            currentAmount = value;
            if (Panel != null) Panel.GetComponentInChildren<TextMeshPro>().text = LabelName;
             } }

    public object Clone() {
        return MemberwiseClone();
    }

    public bool CanInteract(GameObject mayBeOwner)
    {
        return owner == null || owner == mayBeOwner || (owner.CompareTag("Player") && !playerMetaData.privateItems);
    }

    public ItemData CopyItem()
    {

        var b = new ItemData
        {
            canBeInHandsAlso = canBeInHandsAlso,
            Title = Title,
            Description = Description,
            MaxAmount = MaxAmount,
            CurrentAmount = CurrentAmount,
            Type = Type,
            name = name,
            Icon = Icon,
            PrefabName = PrefabName,
            inHandsPrefab = inHandsPrefab,
            owner = owner
        };
        return (b);
    }

    public void ShowLabel(GameObject player) {
        Panel.GetComponentInChildren<TextMeshPro>().color = CanInteract(player) ? colorStandart : colorRed;
    }
}

public enum Category {
    Armor, Potion, Other, MetalDetector, Dig
}

public class ItemData
{
    public string[] canBeInHandsAlso;
    public string Title;
    public string name;
    public string Description;
    public int MaxAmount;
    public int CurrentAmount = 1;
    public Category Type;
    public Sprite Icon;
    public string PrefabName;
    public string[] inHandsPrefab;
    public GameObject owner;

    public ItemData GetCopy()
    {
        var copy = new ItemData
        {
            canBeInHandsAlso = canBeInHandsAlso,
            Title = Title,
            Description = Description,
            MaxAmount = MaxAmount,
            CurrentAmount = CurrentAmount,
            Type = Type,
            name = name,
            Icon = Icon,
            PrefabName = PrefabName,
            inHandsPrefab = inHandsPrefab,
            owner = owner
        };
        return copy;
    }

}

public interface ICanBeOwn
{
    GameObject Owner { get; set; }
    bool CanInteract(GameObject mayBeOwner);
}