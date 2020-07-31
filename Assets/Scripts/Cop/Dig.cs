using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dig : MonoBehaviour {

    [SerializeField] GameObject digFrame;
    GameObject currentFrame;
    public bool isLocal;
    GameObject pl;
    PlayerNet plNet;
    HealthBar bar;
    Moving mov;
    Vector3 v1 = new Vector3(0, 0, -0.01f);
    Vector3 v2 = new Vector3(0, 0, -0.02f);

    BlockController block = null;
    Transform currentStandartBlock = null;
    bool inited;

    // Use this for initialization
    public void End () {
        Destroy(currentFrame);
    }

    void Start()
    {
        pl = transform.parent.gameObject;
        plNet = pl.GetComponent<PlayerNet>();
        if (plNet == null) return;
        mov = plNet._moving;
        bar = plNet.healthBar;
        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!inited) return;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("StandartBlock"))
            {
                if (currentStandartBlock == hit.transform)
                {
                    CheckPress();
                    return;
                }
                currentStandartBlock = hit.transform;

                if (currentFrame != null) currentFrame.transform.position = hit.transform.position + v1;
                else currentFrame = Instantiate(digFrame, hit.transform.position + v2, Quaternion.identity);
                //GameObject.Find("LocalPlayer").GetComponent<PlayerNet>().CmdPrintToServer("123");


                block = hit.collider.GetComponent<BlockController>();
                if (block == null)
                {
                    RaycastHit2D[] hits2 = Physics2D.RaycastAll(transform.position, Vector2.zero);
                    var currentZone = "";
                    foreach (var hit2 in hits2)
                    {
                        if (hit2.collider.CompareTag("CopZone"))
                        {
                            currentZone = hit2.collider.name;
                            break;
                        }
                    }
                    plNet.CmdSetWhole(hit.collider.transform.position, currentZone);
                }
                CheckPress();
                return;

            }
        }
        //GameObject.Find("LocalPlayer").GetComponent<PlayerNet>().CmdPrintToServer("333");

        if (currentFrame != null)
        {
            Destroy(currentFrame);
            currentFrame = null;
            currentStandartBlock = null;
            return;
        }
    }

    public void CheckPress()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isLocal && currentStandartBlock != null)
        {
            if (block == null)
            {
                block = currentStandartBlock.GetComponent<BlockController>();
                if (block == null) return;
            }
            if (!block.CurrentWhole.IsEnd() && bar.Energy > 4)
            {
                //Создать у всех видимую шкалу, а у этого только действующую
                mov.SetScaleInHands(2, bar.Energy, true, 0, block.CurrentWhole);
                plNet.cmd.CmdSetScaleInHands(pl, 2, bar.Energy, 0);//создаем шкалу у всех
            }
        }
    }

    void OnDestroy()
    {
        if (currentFrame != null) Destroy(currentFrame);
    }
}
