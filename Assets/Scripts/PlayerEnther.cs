using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnther : MonoBehaviour {

    [SerializeField] private GameObject currentObject;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            SetTransparency(0.7f, 0.7f, currentObject);
            SetTransparency(0.7f, 0.7f, col.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            SetTransparency(0.5f, 0.7f, currentObject);
            SetTransparency(0.5f, 0.7f, col.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            SetTransparency(1, 1, currentObject);
            SetTransparency(1, 1, col.gameObject);
        }
    }

    void SetTransparency(float first, float second, GameObject needObject)
    {
        var objectRender = needObject.GetComponent<SpriteRenderer>();
        if (objectRender != null)
            objectRender.color = new Color(objectRender.color.r, objectRender.color.g, objectRender.color.b, first);
        foreach (Transform eachChild in needObject.transform)
        {
            SpriteRenderer eachChildRender;
            if (eachChild.name == "Body")
            {
                foreach (Transform eachChild2 in eachChild.transform)
                {
                    eachChildRender = eachChild2.GetComponent<SpriteRenderer>();
                    eachChildRender.color = new Color(eachChildRender.color.r, eachChildRender.color.g, eachChildRender.color.b, second);

                }
            }
            else
            {
                var ch = eachChild.GetAllAllChilds(true);
                foreach (var c in ch)
                {
                    if (eachChildRender = c.GetComponent<SpriteRenderer>())
                    {
                        eachChildRender.color = new Color(eachChildRender.color.r, eachChildRender.color.g, eachChildRender.color.b, second);
                    }
                }
            }
        }
    }
}