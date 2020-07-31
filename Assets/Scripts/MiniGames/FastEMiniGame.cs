using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class FastEMiniGame : StandartMinigame
{

    [SerializeField] GameObject scale = null;
    [SerializeField] GameObject negariveScale;
    [SerializeField] Transform mainScale;

    private float width = 1.4f;
    private float standartScale;

    int position = 0;
    public float negativePos = 0;
    public float difficulty = 2;
    bool rightMoving = true;
    protected KeyCode[] usingKeyCodes = new KeyCode[] { KeyCode.E, KeyCode.None };

    void Start()
    {
        standartScale = 0.2f - 0.02f * difficulty;
        ChangeScale();
        ChangeScalePosition();
    }

    protected override KeyCode[] GetUsingKeyKodes()
    {
        return usingKeyCodes;
    }

    void ChangeScale()
    {
        var newScale = Random.Range(-0.05f, 0.05f);
        scale.transform.localScale = new Vector3(standartScale + newScale, 1, 1);
    }

    void ChangeScalePosition()
    {
        var randomPlusPositon = Random.Range(0, width - scale.transform.localScale.x * width);
        scale.transform.position = new Vector3(randomPlusPositon + mainScale.position.x, scale.transform.position.y, scale.transform.position.z);
    }

    void FixedUpdate()
    {
        TimerTick();

        if (Input.GetKeyDown(KeyCode.E))
        {

            if (negariveScale.transform.position.x >= scale.transform.position.x && negariveScale.transform.position.x <= scale.transform.position.x + scale.transform.localScale.x * width)
            {
                bar.AddEnergy(-3);
                Finish();
            }
            else
                bar.AddEnergy(-3);
            ChangeScale();
            ChangeScalePosition();
            CheckEndEnergy();
        }

        negativePos += 4 * Time.fixedDeltaTime / (difficulty * 2) * (rightMoving ? 1 : -1);
        negariveScale.transform.position = new Vector3(mainScale.transform.position.x + width * negativePos, negariveScale.transform.position.y, negariveScale.transform.position.z);

        if (negativePos >= 1)
        {
            rightMoving = false;

        }
        else if (negativePos <= 0)
        {
            rightMoving = true;
        }
    }
}