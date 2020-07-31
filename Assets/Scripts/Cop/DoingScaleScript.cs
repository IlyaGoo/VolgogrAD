using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoingScaleScript : MonoBehaviour
{
    static float needDelay = 2;
    float currentTimer = needDelay;
    GameObject player;
    public IScaleDoing whole;
    [SerializeField] Transform scale;
    [SerializeField] SpriteRenderer scaleRender;
    bool skiped = false;
    bool localController = false;
    public bool allradyDestroying = false;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] clips;

    public void Init(GameObject pl, IScaleDoing bl, int energy, bool isController, int soundNum, float time = 2)
    {
        source.clip = clips[soundNum];
        source.Play();
        localController = isController;
        needDelay = time;
        currentTimer = time;
        player = pl;
        //var energy = pl.GetComponent<HealthBar>().Energy;
        if (energy > 50)
            scaleRender.color = new Color(1 - (energy-50)/50f, 1, 0);
        else
            scaleRender.color = new Color(1, energy / 50f, 0);
        whole = bl;
    }

    private void OnGUI()
    {
        if (!skiped || !localController || allradyDestroying)
        {
            return;
        }

        if (whole != null && Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
        {
            //var e = Event.current;
            Close();
            var mov = player.GetComponent<Moving>();
            mov.objectsInHands.Remove(GetComponent<ObjectInHands>());
            mov.inHandsNames.Remove("DoingDelayScale");
            player.GetComponent<Commands>().CmdDestroyScaleInHands(player);
            //GameObject.Find("LocalPlayer").GetComponent<Commands>().CmdPrintEverywhere(e.keyCode.ToString());
            return;
        }
    }

    void Update()
    {
        if (!skiped)
        {
            skiped = true;
            return;
        }

        currentTimer -= Time.deltaTime;
        scale.transform.localScale = new Vector3(1 - currentTimer / needDelay, 1, 1);
        if (currentTimer <= 0)
            Do();
    }

    public void Do()
    {
        if (whole != null)
        {
            whole.ScaleDo();
        }
        Close();
    }

    public void Close()
    {
        allradyDestroying = true;//Нужно, чтобы шкала не вызвала второй раз метод дестроинга шкалы в OnGUI
        var mov = player.GetComponent<Moving>();
        mov.objectsInHands.Remove(GetComponent<ObjectInHands>());
        mov.inHandsNames.Remove("DoingDelayScale");
        //mov.dontBeReflect.Remove(gameObject);
        Destroy(gameObject);
    }
}
