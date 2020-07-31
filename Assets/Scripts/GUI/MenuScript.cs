using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour {
    [SerializeField] private GameObject backButton;

    [SerializeField] private GameObject generalButtons;
    [SerializeField] private GameObject characterButtons;
    [SerializeField] private GameObject optionsButtons;
    [SerializeField] private GameObject multiplayerButtons = null;

    [SerializeField] private TMP_InputField currentNickname;
    private string nickname;

    private Sprite[] headSprites;
    private int headNum = 0;
    private Sprite[] bodySprites;
    private int bodyNum = 0;
    private Sprite[] legsSprites;
    private int legsNum = 0;
    [SerializeField] private Image headSprite;
    [SerializeField] private Image bodySprite;
    [SerializeField] private Image legsSprite;

    [SerializeField] private TMP_Text headDescriptionText;
    [SerializeField] private TMP_Text bodyDescriptionText;
    [SerializeField] private TMP_Text legsDescriptionText;

    [SerializeField] private GameObject Host;
    [SerializeField] private GameObject Connect;
    [SerializeField] private GameObject Play;
    [SerializeField] private GameObject ConnectLocal;

    //Ниже номера доступных для игрока частей тела
    readonly List<int> headNums = new List<int>() { 0, 1, 2 };
    readonly List<int> bodyNums = new List<int>() { 0, 1, 2, 4 };
    readonly List<int> legsdNums = new List<int>() { 0, 1 };

    PlayerMetaData _meta;
    public PlayerMetaData Meta
    {
        get
        {
            if (_meta == null)
            {
                _meta = GameObject.Find("PlayerMetaData").GetComponent<PlayerMetaData>();
            }
            return _meta;
        }
    }

    public int HeadNum { get => headNum; set { headNum = value; Meta.headNum = value; } }
    public int BodyNum { get => bodyNum; set { bodyNum = value; Meta.bodyNum = value; } }
    public int LegsNum { get => legsNum; set { legsNum = value; Meta.legsNum = value; } }
    public string Nickname { get => nickname; set { nickname = value; Meta.nickname = value; } }

    readonly string[] headDescription = { "Илья\nКогда-нибудь будет давать бонусы, в том числе повышенный шанс нахождения хвостов",
        "Ромчик\nКогда-нибудь будет давать бонусы, в том числе к готовке",
        "Лысый Ромчик\nКогда-нибудь будет давать бонусы, в том числе к готовке и Аням" };

    readonly string[] bodyDescription = { "Зеленая рубашка\nКогда-нибудь будет давать бонусы, в том числе устойчивость к температуре",
        "Черная футболка\nЛучше не надевать на жаре",
        "Зеленая футболка\nБессмертна",
        "Футболка \"Вахта памяти\"\nЧе поисковик?"};

    readonly string[] legsDescription = { "Хаки штаны\nКогда-нибудь будет давать бонусы, в том числе скрытность",
        "Синие боюки\nКогда-нибудь будет давать бонусы, в том числе секс с малолетками"};

    string docPath = "";

    private void Start()
    {
        var networkManager = GameObject.Find("NetworkManager").GetComponent<NetWorkManager_Custom>();
        networkManager.SetLoading(false);
        headSprites = new Sprite[headNums.Count];
        bodySprites = new Sprite[bodyNums.Count];
        legsSprites = new Sprite[legsdNums.Count];
        for(var i = 0; i < headNums.Count; i++)
        {
            headSprites[i] = Resources.LoadAll<Sprite>("SpritesForBody/Head" + headNums[i])[0];
        }
        for (var i = 0; i < bodyNums.Count; i++)
        {
            bodySprites[i] = Resources.LoadAll<Sprite>("SpritesForBody/Body" + bodyNums[i] + "Back")[1];
        }
        for (var i = 0; i < legsdNums.Count; i++)
        {
            legsSprites[i] = Resources.LoadAll<Sprite>("SpritesForBody/Lags" + legsdNums[i] + "Front")[1];
        }

        Cursor.visible = true;
        Host.GetComponent<Button>().onClick.RemoveAllListeners();
        Host.GetComponent<Button>().onClick.AddListener(networkManager.StartupHost);
        Play.GetComponent<Button>().onClick.RemoveAllListeners();
        Play.GetComponent<Button>().onClick.AddListener(networkManager.StartupHost);
        ConnectLocal.GetComponent<Button>().onClick.RemoveAllListeners();
        ConnectLocal.GetComponent<Button>().onClick.AddListener(networkManager.JoinLocalGame);
        Connect.GetComponent<Button>().onClick.RemoveAllListeners();
        Connect.GetComponent<Button>().onClick.AddListener(networkManager.JoinGame);

        docPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/VolgogrAD";
        Directory.CreateDirectory(docPath);
        Directory.CreateDirectory(docPath + "/Saves");
        if (File.Exists(docPath + "/player.txt"))
        {
            using (StreamReader sw = new StreamReader(docPath + "/player.txt", System.Text.Encoding.Default))
            {
                currentNickname.text = sw.ReadLine();
                Nickname = currentNickname.text;
                var bodyNumsText = sw.ReadLine().Split(',');
                HeadNum = headNums.IndexOf(int.Parse(bodyNumsText[0]));
                BodyNum = bodyNums.IndexOf(int.Parse(bodyNumsText[1]));
                LegsNum = legsdNums.IndexOf(int.Parse(bodyNumsText[2]));
            }
        }
        else
        {
            try
            {
                var a = File.Create(docPath + "/player.txt");
                a.Close();
                using (StreamWriter sw = new StreamWriter(docPath + "/player.txt", false, System.Text.Encoding.Default))
                {
                    currentNickname.text = "Дух";
                    Nickname = "Дух";
                    sw.WriteLine(currentNickname.text);
                    sw.WriteLine("0,0,0");
                }
            }
            catch
            {
                currentNickname.text = "Дух";
                Nickname = "Дух";
                Meta.ThroveMassege("Какие-то проблемы с сохранением файлов.\nВозможно не хватает памяти на диске.");
                //TODO хуйнуть надпись, что не вышло сделать файл
            }
        }
        PlayerPrefs.SetString("NickName", currentNickname.text);
        UpdateHeadDescription();
        UpdateBodyDescription();
        UpdateLegsDescription();
    }

    public void AcceptChanges()
    {
        using (StreamWriter sw = new StreamWriter(docPath + "/player.txt", false, System.Text.Encoding.Default))
        {
            Nickname = currentNickname.text;
            sw.WriteLine(Nickname);
            
            sw.WriteLine(headNums[HeadNum] + "," + bodyNums[BodyNum] + "," + legsdNums[LegsNum]);
        }
    }

    public void AddHead()
    {
        HeadNum += 1;
        if (HeadNum == headSprites.Length) HeadNum = 0;
        UpdateHeadDescription();
    }

    public void RemoveHead()
    {
        HeadNum -= 1;
        if (HeadNum == -1) HeadNum = headSprites.Length - 1;
        UpdateHeadDescription();
    }

    private void UpdateHeadDescription()
    {
        headSprite.sprite = headSprites[HeadNum];
        if (HeadNum < headDescription.Length) headDescriptionText.text = headDescription[HeadNum];
        else headDescriptionText.text = "";
    }

    private void UpdateBodyDescription()
    {
        if (BodyNum < bodyDescription.Length) bodyDescriptionText.text = bodyDescription[BodyNum];
        else bodyDescriptionText.text = "";
        bodySprite.sprite = bodySprites[BodyNum];
    }

    private void UpdateLegsDescription()
    {
        if (LegsNum < legsDescription.Length) legsDescriptionText.text = legsDescription[LegsNum];
        else legsDescriptionText.text = "";
        legsSprite.sprite = legsSprites[LegsNum];
    }

    public void AddBody()
    {
        BodyNum += 1;
        if (BodyNum == bodySprites.Length) BodyNum = 0;
        UpdateBodyDescription();
    }

    public void RemoveBody()
    {
        BodyNum -= 1;
        if (BodyNum == -1) BodyNum = bodySprites.Length - 1;
        UpdateBodyDescription();
    }

    public void AddLegs()
    {
        LegsNum += 1;
        if (LegsNum == legsSprites.Length) LegsNum = 0;
        UpdateLegsDescription();
    }

    public void RemoveLegs()
    {
        LegsNum -= 1;
        if (LegsNum == -1) LegsNum = legsSprites.Length - 1;
        UpdateLegsDescription();
    }
    public void SetNickname()
    {
        _meta.nickname = Nickname = currentNickname.text;
    }

    public void StartLevel(string needLevel)
    {
        SceneManager.LoadScene(needLevel);
    }

    public void Restart()
    {
        PlayerPrefs.SetInt("lvl", 0);
    }

    public void InMneu()
    {
        generalButtons.SetActive(true);
        multiplayerButtons.SetActive(false);
        optionsButtons.SetActive(false);
        backButton.SetActive(false);
        characterButtons.SetActive(false);
    }

    public void OpenOptionsButtons()
    {
        generalButtons.SetActive(false);
        optionsButtons.SetActive(true);
        backButton.SetActive(true);
    }

    public void OpenСharacterButtons()
    {
        generalButtons.SetActive(false);
        characterButtons.SetActive(true);
        backButton.SetActive(true);
    }

    public void OpenMultiplayerButtons()
    {
        generalButtons.SetActive(false);
        multiplayerButtons.SetActive(true);
        backButton.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
