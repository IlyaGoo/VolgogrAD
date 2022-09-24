using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/** Скрипт взаимодействия с окружающим миров
Позволяет брать предметы, входить в идалоги, начинать миниигры
 */
public class Taker : MonoBehaviourExtension, IListener
{
    [SerializeField] Commands cmd;
    [SerializeField] GameObject player = null;
    public GameObject currentAreaDoing;
    public TakerDoing currentDoingType = TakerDoing.Empty;
    public List<GameObject> inTriggerObjects = new List<GameObject>();

    public void Init() {
        localListenersManager.SpaceListeners.Add(this);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.gameObject.tag)
        {
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
            currentDoingType = TakerDoing.Empty;
            return;
        }
        else
        {
            var needTaking = inTriggerObjects[0];
            if (needTaking == null)
            {
                inTriggerObjects.RemoveAt(0);
                TakeFirst();
                return;
            }

            if (needTaking.CompareTag("AreaDoing"))
            {
                currentDoingType = TakerDoing.Area;
                currentAreaDoing = needTaking;
                var areaDo = needTaking.GetComponent<TriggerAreaDoing>();
                areaDo.PlayerThere = true;
                areaDo.TurnLabel(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("AreaDoing")) return;
        int index = inTriggerObjects.IndexOf(col.gameObject);
        if (index != -1)
        {
            inTriggerObjects.RemoveAt(index);

            if (index == 0)
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
        if (currentAreaDoing != null && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
        {
            var areaDoing = currentAreaDoing.GetComponent<TriggerAreaDoing>();
            if (areaDoing.HasWASDReqction) areaDoing.WasdDoing();
        }
    }

    public void EventDid()
    {
        if (currentDoingType == TakerDoing.Area)
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
