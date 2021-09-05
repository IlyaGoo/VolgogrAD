using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Moving : StandartMoving
{
    [SerializeField] AudioSource GoingSource;
    [SerializeField] AudioClip[] goingClips;
    [SerializeField] AudioClip[] swimmingClips;
    private bool m_FacingRight = true;
    public PlayerNet plNet;
    protected override bool DigIsLocal() => isLocalPlayer;
    protected override bool FindIsController() => isLocalPlayer;
    public bool controlled = false;
    public bool initializated = false;

    //todo звучит как какая-то дичь
    public List<GameObject> objectsStopsThrove = new List<GameObject>();//Список объектов запрещающих выбрасывать что-то из инвентаря
    public bool CanThrove => objectsStopsThrove.Count == 0;

    void FixedUpdate()
    {
        if (!isLocalPlayer || stacked) return;
        float xMove = 0;
        float yMove = 0;
        bool nowShifter = false;

        if (!inWater)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (!isShifted)
                {
                    if (currentShiftMultiplayer != null) RemoveSpeedMultiplayer(currentShiftMultiplayer);
                    currentShiftMultiplayer = AddSpeedMultiplayer(shiftMultiplayer);
                }
                nowShifter = true;
            }
            else
            {
                if (isShifted)
                {
                    if (currentShiftMultiplayer != null) RemoveSpeedMultiplayer(currentShiftMultiplayer);
                    currentShiftMultiplayer = null;
                }
                nowShifter = false;
            }
        }

        if (Input.GetKey(KeyCode.A)) xMove = -1;
        else if (Input.GetKey(KeyCode.D)) xMove = 1;
        if (Input.GetKey(KeyCode.W)) yMove = 1;
        else if (Input.GetKey(KeyCode.S)) yMove = -1;

        if (Mathf.Abs(xMove) == 1 && Mathf.Abs(yMove) == 1)
        {
            xMove *= multiplayer;
            yMove *= multiplayer;
        }
        CmdSendFloats(xMove, yMove, currentSpeedMultiplayer, nowShifter);

        if (xMove > 0 && !m_FacingRight)
            Flip();
        else if (xMove < 0 && m_FacingRight)
            Flip();

        //body.AddForce(transform.right *speed * xMove);
        //body.MovePosition(new Vector2((transform.position + transform.right * speed * Time.deltaTime * xMove).x, (transform.position + transform.up * speed * Time.deltaTime * yMove * 1.5f).y));
        transform.position += (transform.up * yMove + transform.right * xMove) * maxSpeed * Time.fixedDeltaTime * currentSpeedMultiplayer;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
    }

    [Command]
    private void CmdSendFloats(float x, float y, float multyplayer, bool shifted)
    {
        RpcSetAnim(x, y, multyplayer, shifted);
    }

    [ClientRpc]
    private void RpcSetAnim(float xMove, float yMove, float multyplayer, bool shifted)
    {
        SetAnim(xMove, yMove, multyplayer, shifted);
        if (xMove != 0 || yMove != 0)
        {
            GoingSource.pitch = 0.4f + 0.6f * multyplayer;
            if (!GoingSource.isPlaying)
            {
                GoingSource.clip = inWater ? swimmingClips[UnityEngine.Random.Range(0, swimmingClips.Length)] : goingClips[UnityEngine.Random.Range(0, goingClips.Length)];
                GoingSource.Play();
            }
        }
        else GoingSource.Stop();
    }

    public override void EnterInWater(int deep)
    {
        base.EnterInWater(deep);
        GoingSource.clip = inWater ? swimmingClips[UnityEngine.Random.Range(0, swimmingClips.Length)] : goingClips[UnityEngine.Random.Range(0, goingClips.Length)];
        //GoingSource.clip = goingClips[deep];
    }

    private void Flip()
    {
        m_FacingRight = !m_FacingRight;
        ChangeScale(-transform.localScale.x);
    }
}
