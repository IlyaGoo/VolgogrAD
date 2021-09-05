using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Taker : MonoBehaviourExtension, IListener
{
    public Commands cmd;
    public InventoryController InventoryRef;
    [SerializeField] GameObject player = null;

    public Skiper skiper;
    public PlayerNet playNet;
    public Moving movingScript;

    public GameObject currentAreaDoing;

    public TakerDoing current_can = TakerDoing.Empty;
    public bool isLocal = false;

    public List<GameObject> inTriggerObjects = new List<GameObject>();

    public void Init() {
        localListenersManager.SpaceListeners.Add(this);
        isLocal = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.gameObject.tag)
        {
            case "TriggerArea":
                cmd.CmdStartGames(col.gameObject);
                break;
            case "AreaDoing":
                {
                    var triggerScript = col.gameObject.GetComponent<TriggerAreaDoing>();
                    if (!triggerScript.CanInteract(player)) return;
                    inTriggerObjects.Add(col.gameObject);
                    if (!triggerScript.needPushButton)
                        triggerScript.Do();
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

            if (nedTaking.CompareTag("AreaDoing"))
            {
                current_can = TakerDoing.Area;
                currentAreaDoing = nedTaking;
                var areaDo = nedTaking.GetComponent<TriggerAreaDoing>();
                areaDo.PlayerThere = true;
                areaDo.TurnLabel(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("AreaDoing")) return;
        int num = inTriggerObjects.IndexOf(col.gameObject);
        if (num != -1)
        {
            inTriggerObjects.RemoveAt(num);

            if (num == 0)
            {
                var areaDo = col.GetComponent<TriggerAreaDoing>();
                if (col.gameObject == currentAreaDoing)
                {
                    areaDo.ExitFrom(player);
                }
                currentAreaDoing = null;
                areaDo.TurnLabel(false);
                areaDo.PlayerThere = false;

                TakeFirst();
            }

        }
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentAreaDoing != null && currentAreaDoing.GetComponent<TriggerAreaDoing>().WasdBool)
                currentAreaDoing.GetComponent<TriggerAreaDoing>().WasdDoing();
        }
    }

    public void EventDid()
    {
        if (current_can == TakerDoing.Area)
        {
            var triggerScript = currentAreaDoing.GetComponent<TriggerAreaDoing>();
            if (triggerScript.needPushButton)
                triggerScript.Do();
        }
    }
}

public enum TakerDoing
{
    Area, Empty
}
