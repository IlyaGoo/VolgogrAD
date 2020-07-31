using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobPositionSinc : Synchronization
{

    private Vector3 _scale = new Vector3(1, 1, 1);

    Commands _commands;

    void Update()
    {
        if (isServer)
        {
            return;
        }

        InterpolatePosition();
        InterpolateScale();
    }

    void FixedUpdate()
    {
        if (!isServer)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y/100);
            return; 
        }

        if (_commands == null)
        {
            try
            {
                _commands = GameObject.Find("LocalPlayer").GetComponent<Commands>();
            }
            catch
            {
                return;
            }
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
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
        transform.localScale = _scale;
    }

    private void InterpolateScale()
    {
        transform.localScale = _lastScale;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_lastRotation), Time.deltaTime * _rotLerpRate);
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
