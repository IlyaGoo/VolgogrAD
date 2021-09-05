using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskMenuScript : MonoBehaviourExtension, IListener
{
    public static TaskMenuScript instance;
    [SerializeField] public GameObject firstButton;
    [SerializeField] public GameObject secondButton;
    [SerializeField] public GameObject mapButton;
    public List<Menu> menues = new List<Menu>();
    public GameObject currentMapObject;

    [SerializeField] public GameObject map;

    public Menu targetMenu;

    void Awake()
    {
        instance = this;
    }

    public bool IsActive => map.activeSelf;

    public void ChangeState(bool state)
    {
        currentMapObject.SetActive(state);
        map.SetActive(state);
        if (state)
        {
            localListenersManager.TabListeners.Add(this);
            localListenersManager.EscListeners.Add(this);
        }
        else
        {
            localListenersManager.TabListeners.Remove(this);
            localListenersManager.EscListeners.Remove(this);
        }
    }

    public void CloseMap()
    {
        ChangeState(false);
        //callM.removeObject(this);
    }

    public void EventDid()
    {
        ChangeState(false);
    }

    public void ChooseMap(Menu needMenu)
    {
        if (currentMapObject != null) currentMapObject.SetActive(false);
        currentMapObject = needMenu.mapScript.mapGameObject;

        ChangeState(true);
        //FindObjectOfType<CallMenu>().ActivateObject(this);
    }

    // Use this for initialization
    public Menu AddMenu (string name, Transform target = null, bool needMap = false, MapScript mapS = null) {
        //заспавнить меню
        float y = -70;
        foreach(var me in menues)
        {
            y -= 70;
            if (!me.hided) y -= me.miniButtons.Count * 70;
        }
        var currentMiniGameObject = Instantiate(firstButton, new Vector3(gameObject.transform.position.x - 60, gameObject.transform.position.y + y, 0), Quaternion.identity, gameObject.transform);

        var newMenu = currentMiniGameObject.GetComponent<Menu>();
        newMenu.menuScript = this;
        if (needMap)//Кнопка открытия графа
        {
            var newButton = Instantiate(mapButton, new Vector3(currentMiniGameObject.transform.position.x+130, currentMiniGameObject.transform.position.y, currentMiniGameObject.transform.position.z), Quaternion.identity, currentMiniGameObject.transform);
            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate { ChooseMap(newMenu); });
        }

        Button btnPlane = currentMiniGameObject.GetComponent<Button>();
        btnPlane.onClick.AddListener(delegate//todo
        {
            ChangeTargerInterface(currentMiniGameObject.GetComponent<ButtonInfo>());
        });

        newMenu.secondButton2 = secondButton;
        newMenu.menues = menues;
        newMenu.mapScript = mapS;
        newMenu.targetTransform = target;
        newMenu.ownMenu = newMenu;
        if (menues.Count == 0)
        {
            targetMenu = newMenu;
            targetMenu.currentTarget = target; 
            newMenu.ChangeTarget(true); 
            newMenu.targetButton = newMenu;
        }//Делаем новое меню таргетным
        newMenu.text = name;

        newMenu.buttonObject = currentMiniGameObject;
        menues.Add(newMenu);
        currentMiniGameObject.GetComponent<TextMeshProUGUI>().text = name;
        return newMenu;
    }

    public void ChangeTargerInterface(ButtonInfo button)
    {
        if (!button.canChange) return;
        if (button.gameController == null) //Случаей, если нажали на кнопку меню, а не на подменю
        {
            button.ChangeTarget(!button.isTarget);
        }
        else
            button.gameController._taskController.ChangeTarget(button.gameController.taskPoint);
    }

    public void RemoveMenu(Menu needRemove)
    {
        int num = menues.IndexOf(needRemove);
        for (var i = num + 1; i < menues.Count; i++)
        {
            menues[i].buttonObject.transform.position = new Vector3(menues[i].buttonObject.transform.position.x, menues[i].buttonObject.transform.position.y + menues[num].heightVal, menues[i].buttonObject.transform.position.z);
        }
        menues.RemoveAt(num);
        if (needRemove.thisMenuTarget) 
        {
            if (menues.Count == 0)
                targetMenu = null; 
            else if (menues[0].miniButtons.Count > 0)
                menues[0].miniButtons[0].ChangeTarget(true);
            else
                menues[0].ChangeTarget(true);
        }
        Destroy(needRemove.buttonObject);
    }
}