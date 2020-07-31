using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectingItem : MonoBehaviour {

    public int depth;
    public int currentDepth;
    public Item itemData;

    public void init(int newDepth, GameObject parent)
    {
        depth = currentDepth = newDepth;
        transform.parent = parent.transform;
        parent.GetComponent<WholeScript>().AddItem(this);
    }
}
