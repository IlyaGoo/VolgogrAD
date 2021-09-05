

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CallMenu : MonoBehaviourExtension, IListener
{
    public static CallMenu instance;
    private bool Pause;
    [SerializeField] private GameObject buttons;
    private bool optionsMenuIsOpenned;
    public NetWorkManager_Custom manager;
    List<IListener> tabListeners;
    List<IListener> escListeners;

    private float delay = 0;

    [SerializeField] private GameObject optionsButtons;

    [SerializeField] private bool needToStop = false;

    private CallMenu()
    { }

    private void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        tabListeners = localListenersManager.TabListeners;
        escListeners = localListenersManager.EscListeners;
    }

    void Start()
    {
         Cursor.visible = false;
    }

    public void OpenOptionsButtons()
    {
        buttons.SetActive(false);
        optionsButtons.SetActive(true);
        optionsMenuIsOpenned = true;
    }

    void OnGUI()
    {
        //print(tabListeners.Count);
/*        if (player != null)
            print(player.GetComponent<ListenersManager>().TabListeners.Count + " : " + tabListeners.Count + " : " +
                player.GetComponent<ListenersManager>().EscListeners.Count + " : " + escListeners.Count);*/
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || tabListeners != null && tabListeners.Count > 1 || escListeners != null && escListeners.Count > 1)
        { 
            Cursor.visible = true;
            delay = 2;
        }
        else if (delay > 0)
        {
            delay -= Time.deltaTime;
            if (delay <= 0)
            {
                Cursor.visible = false;
            }
        }
    }

    public void StartGame()
    {
        tabListeners.Remove(this);
        buttons.SetActive(false);
        Pause = false;
    }

    public void RestartGame()
    {
        localPlayer.transform.position = Vector3.zero;
        StartGame();
    }

    public void InMenu()
    {
        manager.SetLoading(true);
        manager.StopHostOwn();
    }

    public void InMainPartOfMenu()
    {
        buttons.SetActive(true);
        optionsButtons.SetActive(false);
        optionsMenuIsOpenned = false;
    }

    public void EventDid()
    {
        if (optionsMenuIsOpenned)
        {
            InMainPartOfMenu();
            return;
        }
        if (Pause == false)
        {
            buttons.SetActive(true);
            tabListeners.Add(this);
            if (needToStop)
                Time.timeScale = 0;
            Pause = true;
        }
        else
        {
            StartGame();
        }
    }
}
