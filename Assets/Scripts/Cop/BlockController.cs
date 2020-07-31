using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour {
    private WholeScript currentWhole;

    public WholeScript CurrentWhole
    {
        get
        {
            return currentWhole;
        }

        set
        {
            currentWhole = value;
            value.Block = gameObject;
        }
    }
}
