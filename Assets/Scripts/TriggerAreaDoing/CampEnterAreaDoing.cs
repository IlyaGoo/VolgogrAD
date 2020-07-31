using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampEnterAreaDoing : TriggerAreaDoing
{
    [SerializeField] AudioSource source;
    [SerializeField] SpriteRenderer currentRender;
    [SerializeField] Collider2D currentCollider;
    [SerializeField] Sprite openSprite = null;
    [SerializeField] Sprite closeSprite = null;
    public bool isOpen = false;
    readonly string[] labelTexts = new string[2] { "Открыть", "Закрыть" };

    public override bool Do(GameObject player)
    {
        player.GetComponent<Commands>().CmdSetCampEnter(gameObject, !isOpen);
        return true;
    }

    public void SetState(bool newState, bool needSound = true)
    {
        if (needSound)
        {
            if (newState)
            {
                source.timeSamples = 0;
                source.pitch = 1;
            }
            else
            {
                source.timeSamples = source.clip.samples - 1;
                source.pitch = -1;
            }
            source.Play();
        }
        isOpen = newState;
        currentCollider.enabled = !isOpen;
        currentRender.sprite = isOpen ? openSprite : closeSprite;
        ChangeText(labelTexts[newState ? 1 : 0]);
        UpdateTextLabel();
    }
}
