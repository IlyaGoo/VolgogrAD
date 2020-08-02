using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMetaData : MonoBehaviour
{
    public static PlayerMetaData instance;
    public NetWorkManager_Custom netWorkManager;

    [SerializeField] GameObject throwingMassegePrefab;

    public string nickname = "Дух";
    public int headNum = 0;
    public int bodyNum = 0;
    public int legsNum = 0;

    public KeyCode AltCode = KeyCode.LeftAlt;
    public KeyCode SpaceCode = KeyCode.Space;
    public KeyCode FCode = KeyCode.F;
    public KeyCode TabCode = KeyCode.Tab;
    public KeyCode EscapeCode = KeyCode.Escape;
    public KeyCode QCode = KeyCode.Q;

    public bool cheatsAavable = true;

    public int LevelOfQuality = -1;
    public float SoundLevel = -1;
    public float MusicLevel = -1;

    public bool privateItems = false;

    private PlayerMetaData()
    { }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ThroveMassege(string text)
    {
        Instantiate(throwingMassegePrefab, GameObject.FindGameObjectWithTag("Canvas").transform).GetComponent<ThrowingMassege>().SetText(text);
    }
}
