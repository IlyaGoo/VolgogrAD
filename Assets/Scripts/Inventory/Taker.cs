using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Taker : MonoBehaviour, IListener
{
    public Commands cmd;
    public PlayerInventoryController InventoryRef;
    [SerializeField] GameObject player = null;

    public Skiper skiper;
    public PlayerNet playNet;
    public Moving movingScript;

    public GameObject currentAreaDoing;

    private TaskManager _TaskManager;

    public TakerDoing current_can = TakerDoing.Empty;
    public bool isLocal = false;


    public List<GameObject> inTriggerObjects = new List<GameObject>();
    readonly string[] tags = new string[] { "Item", "AreaDoing" };

    public void Init() {
        playNet.listenersManager.SpaceListeners.Add(this);
        _TaskManager = playNet.Task;
        isLocal = true;
        _TaskManager._taker = this;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.gameObject.tag)
        {
            case "Item":
                if (!col.GetComponent<Item>().CanInteract(player)) return;
                inTriggerObjects.Add(col.gameObject);
                if (inTriggerObjects.Count == 1) TakeFirst();
                break;
            case "TriggerArea":
                player.GetComponent<Commands>().CmdStartGames(col.gameObject);
                break;
            case "AreaDoing":
                {
                    inTriggerObjects.Add(col.gameObject);
                    var triggerScript = col.gameObject.GetComponent<TriggerAreaDoing>();
                    if (!triggerScript.needPushButton)
                        triggerScript.Do(player);
                    if (inTriggerObjects.Count == 1) TakeFirst();
                    break;
                }
        }
    }

    void TakeFirst()
    {
        if (inTriggerObjects.Count == 0)
        {
            current_can = TakerDoing.Empty;
            return;
        }
        else
        {
            var nedTaking = inTriggerObjects[0];
            if (nedTaking == null)
            {
                inTriggerObjects.RemoveAt(0);
                TakeFirst();
                return;
            }


            switch (nedTaking.tag)
            {
                case "Item":
                    current_can = TakerDoing.Item;
                    break;
                case "AreaDoing":
                    {
                        current_can = TakerDoing.Area;
                        currentAreaDoing = nedTaking;
                        var areaDo = nedTaking.GetComponent<TriggerAreaDoing>();
                        areaDo.PlayerThere = true;
                        areaDo.TurnLabel(true);
                        break;
                    }
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!tags.Contains(col.tag)) return;
        int num = inTriggerObjects.IndexOf(col.gameObject);
        if(num != -1)
        {
            inTriggerObjects.RemoveAt(num);

            if (num == 0)
            {
                switch (col.tag)
                {
                    case "AreaDoing":
                        var areaDo = col.GetComponent<TriggerAreaDoing>();
                        if (col.gameObject == currentAreaDoing)
                        {
                            areaDo.ExitFrom(player);
                        }
                        currentAreaDoing = null;
                        areaDo.TurnLabel(false);
                        areaDo.PlayerThere = false;
                        break;
                }

                TakeFirst();
            }

        }
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentAreaDoing != null && currentAreaDoing.GetComponent<TriggerAreaDoing>().WasdBool)
                currentAreaDoing.GetComponent<TriggerAreaDoing>().WasdDoing(player);
        }
    }

    public void EventDid()
    {
        switch (current_can)
        {
            case TakerDoing.Item:
                Item item = inTriggerObjects[0].GetComponent<Item>();
                if (InventoryRef.CanPutItem(item.CopyItem()))
                {
                    cmd.CmdPlaySound(0);
                    cmd.CmdTakeItem(inTriggerObjects[0]);
                }
                break;
            case TakerDoing.Area:
                var triggerScript = currentAreaDoing.GetComponent<TriggerAreaDoing>();
                if (triggerScript.needPushButton)
                    triggerScript.Do(player);
                break;
        }
    }
}

public enum TakerDoing
{
    Item, Area, Empty
}
