using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnceRadio : NetworkBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject bluePanel;
    [SerializeField] string musicPath;
    int currentMusicNum = -1;
    float currentMusicTime;
    float timer;
    List<RadioSounder> radioSounders = new List<RadioSounder>();
    public bool currentState;
    [SerializeField] float[] musicSeconds;

    Commands _cmd;

    void Start()
    {
        if (isServer)
            ChangeMusic();
    }

    public Commands Cmd
    {
        get
        {
            if (_cmd == null)
            {
                _cmd = GameObject.Find("LocalPlayer").GetComponent<Commands>();
            }
            return _cmd;
        }
    }

    public void SetState(bool state)
    {
        bluePanel.SetActive(state);
        currentState = state;
    }

    public (int, float) GetMusicNums()
    {
        return (currentMusicNum, timer);
    }

    public AudioClip GetMusic(int num)
    {
        if (num == -1) return null;
        var audio = Resources.Load(musicPath + num) as AudioClip;
        return audio;
        //return musics[num];
    }

    void Update()
    {
        if (!isServer || currentMusicNum == -1)
            return;
        timer += Time.deltaTime;
        if (timer >= currentMusicTime)
            ChangeMusic();
    }

    void ChangeMusic()
    {
        if (musicSeconds.Length == 0) return;
        currentMusicNum = Random.Range(0, musicSeconds.Length);
        currentMusicTime = musicSeconds[currentMusicNum];
        timer = 0;
        foreach (var sounder in radioSounders)
        {
            //sounder.SetMusic(currentMusic, 0);
            Cmd.SendMusic2(sounder.gameObject, currentMusicNum, timer);
        }
    }

    public void AddSounder(RadioSounder sounder)
    {
        if (!radioSounders.Contains(sounder))
            radioSounders.Add(sounder);
        //sounder.SetMusic(currentMusic, timer);

        Cmd.SendMusic2(sounder.gameObject, currentMusicNum, timer);
    }

    public void RemoveSounder(RadioSounder sounder)
    {
        radioSounders.Remove(sounder);
    }
}
