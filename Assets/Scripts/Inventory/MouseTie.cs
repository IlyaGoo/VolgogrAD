﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTie : MonoBehaviour
{
    void Update()
    {
        transform.position = Input.mousePosition;
    }
}
