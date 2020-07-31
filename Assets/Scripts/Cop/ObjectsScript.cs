using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsScript : MonoBehaviour {

    [SerializeField] GameObject[] objects;

    public float botSpeedMultiplayer = 1;

    public string GetRandomObject()
    {
        return "DetectingItems/"  + objects[Random.Range(0 , objects.Length)].name;
    }
}
