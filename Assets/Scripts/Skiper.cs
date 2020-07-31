using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skiper : MonoBehaviour {

    public float skipTime;
    public int maxCount = 1;
    public int currentCount = 0;
    [SerializeField] private bool needChangeSprite = false;
    [SerializeField] private SpriteRenderer generalObject;
    public GameObject mainObject;

    public void addOne(Sprite sprite = null)
    {
        currentCount++;
        generalObject.sprite = sprite;
    }

    public void removeOne()
    {
        currentCount--;
        if (currentCount < 0) currentCount = 0;
        generalObject.sprite = null;
    }

}
