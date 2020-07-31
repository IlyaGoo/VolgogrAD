using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synchronization : NetworkBehaviour
{
    [SerializeField]
    protected float _posLerpRate = 15;
    [SerializeField]
    protected float _scaleLerpRate = 15;
    [SerializeField]
    protected float _posThreshold = 0.1f;
    [SerializeField]
    protected float _scaleThreshold = 0f;

    public Vector3 _lastPosition;

    public Vector3 _lastScale;

    void Start()
    {
        _lastPosition = transform.position;
        _lastScale = transform.localScale;
    }
}
