using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsScript : MonoBehaviour {

    private int LevelOfQuality;

    private float SoundLevel;
    private float MusicLevel;

    [SerializeField] private GameObject[] SoundSprites;
    private GameObject currentSoundSprite;
    [SerializeField] private GameObject[] MusicSprites;
    private GameObject currentMusicSprite;

    [SerializeField] private GameObject SoundSpriteON;
    [SerializeField] private GameObject SoundSpriteOFF;
    [SerializeField] private GameObject MusicSpriteON;
    [SerializeField] private GameObject MusicSpriteOFF;

    public AudioMixer mainMixer;

    [SerializeField]
    private bool needToInit;
    string docPath = "";
    PlayerMetaData _meta;
    public PlayerMetaData Meta
    {
        get
        {
            if (_meta == null)
            {
                _meta = FindObjectOfType<PlayerMetaData>();
            }
            return _meta;
        }
    }

    private void Start()
    {
        if (needToInit) Init();
    }

    public void Init()
    {
        LevelOfQuality = QualitySettings.GetQualityLevel();

        docPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/VolgogrAD";
        if (Meta.SoundLevel == -1)
        {
            Directory.CreateDirectory(docPath);
            if (File.Exists(docPath + "/options.txt"))
            {
                using (StreamReader sw = new StreamReader(docPath + "/options.txt", System.Text.Encoding.Default))
                {
                    SoundLevel = float.Parse(sw.ReadLine());
                    MusicLevel = float.Parse(sw.ReadLine());
                    SetNewValue(SoundLevel, true, false);
                    SetNewValue(MusicLevel, false, false);
                }
            }
            else
            {
                try
                {
                    var a = File.Create(docPath + "/options.txt");
                    a.Close();
                    SetNewValue(Meta.SoundLevel, true, false);
                    SetNewValue(Meta.MusicLevel, false, true);
                }
                catch
                {
                    SetNewValue(0.5f, true, false);
                    SetNewValue(0.5f, false, false);
                    //TODO хуйнуть надпись, что не вышло сделать файл
                }
            }
        }
        else//Загрузить из меты
        {
            SetNewValue(Meta.SoundLevel, true, false);
            SetNewValue(Meta.MusicLevel, false, true);
        }
    }

    public void ClickOnSoundButton(float deppend)
    {
        var mouseXPosition = Input.mousePosition.x - deppend;
        SetNewValue(mouseXPosition / 230, true);
    }

    void SetNewValue(float value, bool isSound, bool needSave = true)
    {
        if (isSound)
        {
            SoundLevel = value;
            if (SoundLevel > 1) SoundLevel = 1;
            else if (SoundLevel < 0) SoundLevel = 0;
            Meta.SoundLevel = SoundLevel;
            SetNewSoundSprite();
            SetSound();
        }
        else
        {
            MusicLevel = value;
            if (MusicLevel > 1) MusicLevel = 1;
            else if (MusicLevel < 0) MusicLevel = 0;
            Meta.MusicLevel = MusicLevel;
            SetNewMusicSprite();
            SetMusic();
        }
        if (needSave)
            WriteInOptionsFile();
        SetImageState();
    }

    public void WriteInOptionsFile()
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(docPath + "/options.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(SoundLevel);
                sw.WriteLine(MusicLevel);
            }
        }
        catch { }
    }

    private void SetNewSoundSprite()
    {
        if (currentSoundSprite != null)
            currentSoundSprite.SetActive(false);
        currentSoundSprite = SoundSprites[(int)(SoundLevel * (SoundSprites.Length - 1))];
        currentSoundSprite.SetActive(true);
    }

    private void SetNewMusicSprite()
    {
        if (currentMusicSprite != null)
            currentMusicSprite.SetActive(false);
        currentMusicSprite = MusicSprites[(int)(MusicLevel * (MusicSprites.Length - 1))];
        currentMusicSprite.SetActive(true);
    }

    public void ChangeSoundState()
    {
        //использовать ? :
        if (SoundLevel == 0)
        {
            SoundLevel = 0.5f;
        }
        else
        {
            SoundLevel = 0;
        }
        SetNewValue(SoundLevel, true);
    }

    public void ChangeMusicState()
    {
        //использовать ? :
        if (MusicLevel == 0)
        {
            MusicLevel = 0.5f;
        }
        else
        {
            MusicLevel = 0;
        }
        SetNewValue(MusicLevel, false);
    }

    public void ClickOnButon(int num, float deppend)
    {
        if (num == 0)
        {
            var mouseXPosition = Input.mousePosition.x - deppend;
            SetNewValue(mouseXPosition / 230, true, false);
        }
        else if (num == 1)
        {
            var mouseXPosition = Input.mousePosition.x - deppend;
            SetNewValue(mouseXPosition / 230, false, false);
        }
    }

    private void SetImageState()
    {
        SoundSpriteOFF.SetActive(SoundLevel == 0);
        SoundSpriteON.SetActive(SoundLevel != 0);
        MusicSpriteOFF.SetActive(MusicLevel == 0);
        MusicSpriteON.SetActive(MusicLevel != 0);
    }

    public void SetSound()
    {
        mainMixer.SetFloat("soundsVol", SoundLevel * 100 - 80);
    }

    public void SetMusic()
    {
        mainMixer.SetFloat("musicVol", MusicLevel * 100 - 80);
    }

    public void SetQuality(int newLevelOfQuality)
    {
        QualitySettings.SetQualityLevel(newLevelOfQuality, true);
        LevelOfQuality = newLevelOfQuality;
    }
}
