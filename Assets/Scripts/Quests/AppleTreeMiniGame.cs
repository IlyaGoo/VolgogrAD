using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleTreeMiniGame : StandartMinigame
{

    [SerializeField] GameObject scale;
    [SerializeField] SpriteRenderer[] buttonsRenders;
    int position = 0;
    public float difficulty = 2;

    Color color05 = new Color(0.5f, 0.5f, 0.5f);
    Color color1 = new Color(1, 1, 1);
    Vector3 vector011 = new Vector3(0, 1, 1);
    protected KeyCode[] usingKeyCodes = new KeyCode[] { KeyCode.E, KeyCode.Q, KeyCode.None };

    void Start()
    {
        buttonsRenders[1].material.color = color05;
    }

    protected override KeyCode[] GetUsingKeyKodes()
    {
        return usingKeyCodes;
    }

    protected override void AddOnGUI()
    {
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
                Finish();
            }
        }
    }

    void FixedUpdate()
    {
        TimerTick();
        scale.transform.localScale = new Vector3(Mathf.Max(0, scale.transform.localScale.x - Time.fixedDeltaTime / difficulty), scale.transform.localScale.y, scale.transform.localScale.z);
    }
}