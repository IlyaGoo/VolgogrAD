using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetallDetectorScript : MonoBehaviourExtension {

    [SerializeField] float radius = 0;
    List<GameObject> detectingItems = new List<GameObject>();
    bool isControllerPlayer;
    [SerializeField] int depthZone = 1;
    string currentZone = "";

    Vector4 zone1 = new Vector4(1, 0, 0, 1);
    Vector4 zone3 = new Vector4(1, 0.6f, 0, 1);
    Vector4 zone2 = new Vector4(1, 0.3f, 0, 1);

    public void Init(bool isController)
    {
        isControllerPlayer = isController;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Detecting"))
        {
            if (detectingItems.Contains(col.gameObject)) return;
            detectingItems.Add(col.gameObject);
        }
        else if (col.gameObject.CompareTag("StandartBlock"))
        {
            if (!isControllerPlayer) return;
            if (col.gameObject.GetComponent<BlockController>() == null)
            {
                localPlayerNet.CmdSetWhole(col.transform.position, currentZone);
            }
        }
        else if (col.gameObject.CompareTag("CopZone"))
        {
            currentZone = col.name;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Detecting"))
        {
            var detItemComponent = col.GetComponent<DetectingItem>();
            var dif = depthZone - detItemComponent.currentDepth;
            if (dif < 0) return;
            var eachChildRender = col.GetComponent<SpriteRenderer>();

            switch (detItemComponent.currentDepth)
            {
                case 1:
                    eachChildRender.color = zone1;
                    break;
                case 2:
                    eachChildRender.color = zone2;
                    break;
                case 3:
                    eachChildRender.color = zone3;
                    break;
            }



            eachChildRender.enabled = true;

            var difVector = new Vector2(transform.position.x - col.transform.position.x, transform.position.y - col.transform.position.y);
            var distance = difVector.magnitude - radius;
            if (distance <= 0) 
            { 
                eachChildRender.color = new Color(eachChildRender.color.r, eachChildRender.color.g, eachChildRender.color.b, 1);
            }
            else
            {
                eachChildRender.color = new Color(eachChildRender.color.r, eachChildRender.color.g, eachChildRender.color.b, Mathf.Max(0, 1 - distance / 0.15f));
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Detecting"))
        {
            col.GetComponent<SpriteRenderer>().enabled = false;
            detectingItems.Remove(col.gameObject);
        }
        else if (col.CompareTag("CopZone"))
        {
            currentZone = "";
        }
    }

    public void CloseAll()
    {
        foreach(var el in detectingItems)
        {
            el.GetComponent<SpriteRenderer>().enabled = false;
        }
        detectingItems.Clear();
    }

}
