using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

/** Скрипт взаимодействия с окружающим миров
Позволяет брать предметы, входить в идалоги, начинать миниигры
 */
public class Taker : MonoBehaviourExtension, IListener
{
    [SerializeField] Commands cmd;
    [SerializeField] GameObject player = null;
    public GameObject currentAreaDoing;
    public TakerDoing currentDoingType = TakerDoing.Empty;
    public List<GameObject> availableTriggers = new();
    public List<GameObject> allTriggerObjects = new();

    public void Init() {
        localListenersManager.SpaceListeners.Add(this);
    }

    /** Если что-то поменялось со степами квеста, то необходимо обновить campObject-ы, на которых сейчас стоим */
    public void UpdateCampTriggerArea(CampObjectType campType)
    {
        var needRemove = new List<GameObject>();
        foreach (var inTriggerObject in allTriggerObjects)
        {
            var campComponent = inTriggerObject.GetComponent<CampObject>();
            if (campComponent != null && campComponent.objectType == campType)
            {
                if (availableTriggers.Contains(inTriggerObject))
                {
                    if (!campComponent.CanInteract(player))
                        needRemove.Add(campComponent.gameObject);
                    else
                    {
                        campComponent.UpdateText();
                        campComponent.UpdateTextLabel();
                    }
                }
                else if (!availableTriggers.Contains(campComponent.gameObject) && campComponent.CanInteract(player))
                {
                    campComponent.UpdateText();
                    AddToAvailable(campComponent);
                }
            }
        }

        foreach (var removing in needRemove)
        {
            RemoveFromAvailable(removing);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("AreaDoing")) return;

        var triggerScript = col.gameObject.GetComponent<TriggerAreaDoing>();
        triggerScript.UpdateText();//Обновляем текст, потому что вероятно поменялось много с последнего контакта
        allTriggerObjects.Add(col.gameObject);
        if (!triggerScript.CanInteract(player)) return;
        AddToAvailable(triggerScript);
    }

    private void AddToAvailable(TriggerAreaDoing triggerScript)
    {
        availableTriggers.Add(triggerScript.gameObject);
        if (!triggerScript.needPushButton)
            triggerScript.Do();
        if (availableTriggers.Count == 1) TakeFirst();
    }

    private void TakeFirst()
    {
        if (availableTriggers.Count == 0)
        {
            currentDoingType = TakerDoing.Empty;
            return;
        }

        var needTaking = availableTriggers[0];
        if (needTaking == null)
        {
            availableTriggers.RemoveAt(0);
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

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("AreaDoing")) return;
        allTriggerObjects.Remove(col.gameObject);
        RemoveFromAvailable(col.gameObject);
    }

    private void RemoveFromAvailable(GameObject areaObject)
    {
        var index = availableTriggers.IndexOf(areaObject);
        if (index == -1) return;
        
        availableTriggers.RemoveAt(index);

        if (index == 0)
        {
            var areaDo = areaObject.GetComponent<TriggerAreaDoing>();
            if (areaObject == currentAreaDoing)
            {
                areaDo.ExitFrom(player);
            }
            currentAreaDoing = null;
            areaDo.TurnLabel(false);
            areaDo.PlayerThere = false;

            TakeFirst();
        }
    }

    void Update ()
    {
        if (currentAreaDoing != null && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)  || Input.GetKeyDown(KeyCode.Space)))
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
