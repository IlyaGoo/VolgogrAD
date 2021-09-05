using UnityEngine;
using System;
using TMPro;

public class Item : TriggerAreaDoing, ICloneable, IAltLabelShower
{
    public string Title;
    public string[] canBeInHandsAlso;
    [Multiline(5)]
    public string Description;
    public int MaxAmount;
    private int currentAmount = 1;
    public ItemCategory Type;
    public Sprite Icon;
    public string PrefabName;
    public string[] inHandsPrefab;
    public string[] inHandsPrefabForBots;
    public int itemQality;
    public bool twoHanded;

    Vector3 offset = Vector3.down;

    Color colorStandart = new Color(1, 1, 1, 1);
    Color colorRed = new Color(1, 0, 0, 1);

    public Vector3 Offset { get { 
            if (offset.y == -1) offset = new Vector3(0, GetComponent<BoxCollider2D>().size.y, 0); //GetComponent<BoxCollider2D>().size.x / 2
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

    public override bool CanInteract(GameObject interactEntity)
    {
        return owner == null || owner == interactEntity || (owner.CompareTag("Player") && !playerMetaData.privateEnable);
    }

    public ItemData CopyItem()
    {

        return new ItemData
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
            owner = owner,
            itemQality = itemQality,
            twoHanded = twoHanded,
            inHandsPrefabForBots = inHandsPrefabForBots
        };
    }

    public void ShowLabel(GameObject player) {
        Panel.GetComponentInChildren<TextMeshPro>().color = CanInteract(player) ? colorStandart : colorRed;
    }

    public string[] GetMobsInHands()
    {
        if (inHandsPrefabForBots == null || inHandsPrefabForBots.Length == 0)
            return inHandsPrefab;
        else
            return inHandsPrefabForBots;
    }

    public override bool Do()
    {
        if (localPlayerInventoryController.CanPutItem(CopyItem()))
        {
            localCommands.CmdPlaySound(0);
            localCommands.CmdTakeItem(gameObject);
            return true;
        }
        return false;
    }
}

public enum ItemCategory {
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
    public ItemCategory Type;
    public Sprite Icon;
    public string PrefabName;
    public string[] inHandsPrefab;
    public string[] inHandsPrefabForBots;
    public GameObject owner;
    public int itemQality;//Нужно для определения, что лучше, боты, например, будут брать металлодетектор с большим значением
    public bool twoHanded;

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
            owner = owner,
            itemQality = itemQality,
            twoHanded = twoHanded,
            inHandsPrefabForBots = inHandsPrefabForBots
        };
        return copy;
    }

    public string[] GetMobsInHands()
    {
        if (inHandsPrefabForBots == null || inHandsPrefabForBots.Length == 0)
            return inHandsPrefab;
        else
            return inHandsPrefabForBots;
    }

}

public interface ICanBeOwn
{
    GameObject Owner { get; set; }
    bool CanInteract(GameObject interactEntity);
}