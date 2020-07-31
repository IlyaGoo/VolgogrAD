using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSyn : Synchronization
{

    public Commands _commands;

    private Vector3 _scale = new Vector3(1, 1, 1);
    public bool isDriver;

    void Update()
    {
        if (isDriver)
            return;

        InterpolatePosition();
        InterpolateScale();
    }

    void FixedUpdate()
    {
        if (!isDriver)
        { 
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
            return;
        }

        var posChanged = IsPositionChanged();

        if (posChanged)
        {
            _commands.CmdSendPosition(transform.position, gameObject);
            _lastPosition = transform.position;
        }

        var scaleChanged = IsScaleChanged();

        if (scaleChanged)
        {
            _commands.CmdSendScale(transform.localScale, gameObject);
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

    [Command(channel = Channels.DefaultUnreliable)]
    private void CmdSendPosition(Vector3 pos)
    {
        _lastPosition = pos;
        //_scale = transform.localScale;
    }

    [Command(channel = Channels.DefaultUnreliable)]
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
        return Channels.DefaultUnreliable;
    }

    public float GetNetworkSendInterval()
    {
        return 0.05f;
    }
}
