using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject currentObject;
    [SerializeField] Moving moving;
    public Commands cmd;
    [SerializeField] PlayerInventoryController inventoryController;
    int deep;
    int deep2;
    public bool isController = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.gameObject.tag)
        {
            case "Camp":
                SetTransparency(0.7f, 0.7f, currentObject);
                col.transform.parent.GetComponent<Camp>().AddPlayer();
                /*SetTransparency(0.7f, 0.7f, col.transform.parent.gameObject, false);*/
                break;
            case "WaterArea":
                if (!isController)
                    return;
                deep = 1;
                if (deep2 == 0)
                    deep2 = 1;
                if (moving != null && !moving.objectsStopsThrove.Contains(col.gameObject))
                    moving.objectsStopsThrove.Add(col.gameObject);
                cmd.CmdEnterInWater(currentObject, 1);
                inventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
                break;
            case "WaterDeepArea":
                if (!isController)
                    return;
                deep = 2;
                if (deep2 == 0)
                    deep2 = 2;
                if (moving != null && !moving.objectsStopsThrove.Contains(col.gameObject))
                    moving.objectsStopsThrove.Add(col.gameObject);
                cmd.CmdEnterInWater(currentObject, 2);
                inventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь, на случай если тпшнулись
                break;
        }
    }

/*    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Camp"))
        {
            //SetTransparency(0.5f, 0.7f, currentObject);
            SetTransparency(0.5f, 0.7f, col.transform.parent.gameObject, false);
        }
    }*/

    void OnTriggerExit2D(Collider2D col)
    {
        switch (col.gameObject.tag)
        {
            case "Camp":
                SetTransparency(1, 1, currentObject);
                col.transform.parent.GetComponent<Camp>().RemovePlayer();
                /*SetTransparency(1, 1, col.transform.parent.gameObject, false);*/
                break;
            case "WaterArea":
                if (!isController)
                    return;
                if (moving != null && moving.objectsStopsThrove.Contains(col.gameObject))
                    moving.objectsStopsThrove.Remove(col.gameObject);
                if (deep == 1)
                {
                    if (deep2 == 1)
                    {
                        deep = 0;
                        deep2 = 0;
                        inventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
                        inventoryController.inventories[0].BackInHands();//Возвращаем в руки все
                        cmd.CmdEnterInWater(cmd.gameObject, 0);
                    }
                    else if (deep2 == 2)
                    {
                        deep = 2;
                        cmd.CmdEnterInWater(currentObject, 2);
                        inventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь, на случай если тпшнулись
                    }
                }
                else if (deep == 2)
                {
                    deep2 = 2;
                }
                break;
            case "WaterDeepArea":
                if (!isController)
                    return;
                if (moving != null && moving.objectsStopsThrove.Contains(col.gameObject))
                    moving.objectsStopsThrove.Remove(col.gameObject);
                if (deep == 2)
                {
                    if (deep2 == 2)
                    {
                        deep = 0;
                        inventoryController.inventories[0].SetFreeze(false);//Снова включаем панель предметов
                        inventoryController.inventories[0].BackInHands();//Возвращаем в руки все
                        cmd.CmdEnterInWater(currentObject, 0);
                    }
                    else if (deep2 == 1)
                    {
                        deep = 1;
                        cmd.CmdEnterInWater(currentObject, 1);
                        inventoryController.inventories[0].SetFreeze(true);//Фризим инвентарь
                    }
                }
                else if (deep == 1)
                {
                    deep2 = 1;
                }
                break;
        }
    }

    void SetTransparency(float first, float second, GameObject needObject, bool transparencyChilds = true)
    {
        var objectRender = needObject.GetComponent<SpriteRenderer>();
        if (objectRender != null)
            objectRender.color = new Color(objectRender.color.r, objectRender.color.g, objectRender.color.b, first);
        if (transparencyChilds)
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
                    if (eachChild.CompareTag("Panel")) continue;
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

public static class TransformExtension
{
    public static List<Transform> GetAllAllChilds(this Transform parent, bool include = false)
    {
        var res = new List<Transform>();
        var needCheck = new List<Transform>() { parent };
        if (include) res.Add(parent);
        while (needCheck.Count > 0)
        {
            var curent = needCheck[0];
            needCheck.RemoveAt(0);
            foreach (Transform ch in curent)
            {
                res.Add(ch);
                needCheck.Add(ch);
            }
        }

        return res;
    }
}