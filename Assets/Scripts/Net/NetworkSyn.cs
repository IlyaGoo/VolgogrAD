using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkSyn : NetworkBehaviour {

    [SerializeField]
    private float _posLerpRate = 15;
    [SerializeField]
    private float _scaleLerpRate = 15;
    [SerializeField]
    private float _posThreshold = 0.1f;
    [SerializeField]
    private float _scaleThreshold = 0f;

    [SyncVar]
    private Vector3 _lastPosition;

    [SyncVar]
    private Vector3 _lastScale;

    private Vector3 _scale = new Vector3(1, 1, 1);

    void Update()
    {
        if (isLocalPlayer)
            return;

        InterpolatePosition();
        InterpolateScale();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        var posChanged = IsPositionChanged();

        if (posChanged)
        {
            CmdSendPosition(transform.position);
            _lastPosition = transform.position;
        }

        var scaleChanged = IsScaleChanged();

        if (scaleChanged)
        {
            CmdSendScale(transform.localScale);
            _lastScale = transform.localScale;
        }
    }

    private void InterpolatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _lastPosition, Time.deltaTime * _posLerpRate);
        transform.localScale = _scale;
    }

    private void InterpolateScale()
    {
        transform.localScale = _lastScale;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_lastRotation), Time.deltaTime * _rotLerpRate);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendPosition(Vector3 pos)
    {
        _lastPosition = pos;
        //_scale = transform.localScale;
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendScale(Vector3 scale)
    {
        _lastScale = scale;
    }


    private bool IsPositionChanged()
    {
        return Vector3.Distance(transform.position, _lastPosition) > _posThreshold;
    }

    private bool IsScaleChanged()
    {
        return Vector3.Distance(transform.localScale, _lastScale) > _scaleThreshold;
    }

    public int GetNetworkChannel()
    {
        return Channels.Unreliable;
    }

    public float GetNetworkSendInterval()
    {
        return 0.05f;
    }
}
