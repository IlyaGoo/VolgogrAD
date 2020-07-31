using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QEminiGame : StandartMinigame
{

    [SerializeField] GameObject scale;
    [SerializeField] SpriteRenderer[] buttonsRenders;
    [SerializeField] GameObject negariveScale;
    int position = 0;
    float negativePos = 0;
    public float difficulty = 2;

    Color color05 = new Color(0.5f, 0.5f, 0.5f);
    Color color1 = new Color(1, 1, 1);
    Vector3 vector011 = new Vector3(0, 1, 1);
    protected KeyCode[] usingKeyCodes = new KeyCode[] { KeyCode.E, KeyCode.Q, KeyCode.None };

    // Use this for initialization
    void Start()
    {
        buttonsRenders[1].material.color = color05;
    }

    protected override KeyCode[] GetUsingKeyKodes()
    {
        return usingKeyCodes;
    }

    void FixedUpdate()
    {
        TimerTick();

        if (position == 0 && Input.GetKeyDown(KeyCode.Q) || position == 1 && Input.GetKeyDown(KeyCode.E))
        {
            scale.transform.localScale = new Vector3(Mathf.Max(0, scale.transform.localScale.x + 0.1f), scale.transform.localScale.y, scale.transform.localScale.z);
            if (position == 0)
            {
                position = 1;
                buttonsRenders[0].material.color = color05;
                buttonsRenders[1].material.color = color1;
            }
            else
            {
                position = 0;
                buttonsRenders[1].material.color = color05;
                buttonsRenders[0].material.color = color1;
            }
            if (scale.transform.localScale.x >= 1)
            {
                scale.transform.localScale = vector011;
                negativePos = 0;
                negariveScale.transform.position = new Vector3(-0.7f, negariveScale.transform.position.y, negariveScale.transform.position.z);
                Finish();
            }
        }
        scale.transform.localScale = new Vector3(Mathf.Max(0, scale.transform.localScale.x - Time.fixedDeltaTime / difficulty), scale.transform.localScale.y, scale.transform.localScale.z);
        negativePos += Time.fixedDeltaTime / (difficulty * 2);
        negariveScale.transform.position = new Vector3(scale.transform.position.x + 1.4f * negativePos, negariveScale.transform.position.y, negariveScale.transform.position.z);

        if (negativePos >= 1)
        {
            scale.transform.localScale = vector011;
            negativePos = 0;
            bar.AddEnergy(-5);
            CheckEndEnergy();
        }
    }
}
