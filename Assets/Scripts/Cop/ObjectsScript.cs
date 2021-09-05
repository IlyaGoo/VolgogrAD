﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsScript : MonoBehaviour {
    public static ObjectsScript instance;
    [SerializeField] GameObject[] objects;

    public float botSpeedMultiplayer = 1;

    private ObjectsScript()
    { }

    private void Awake()
    {
        instance = this;
    }

    public string GetRandomObject()
    {
        return "DetectingItems/"  + objects[Random.Range(0 , objects.Length)].name;
    }
}
