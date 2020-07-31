using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskMenuScript : MonoBehaviour, IListener
{

    [SerializeField] public GameObject firstButton;
    [SerializeField] public GameObject secondButton;
    [SerializeField] public GameObject mapButton;
    public List<Menu> menues = new List<Menu>();
    public GameObject currentMapObject;

    [SerializeField] public GameObject map;
    CallMenu callM = null;

    public Menu targetMenu;

    void Start()
    {
        callM = FindObjectOfType<CallMenu>();
    }

    public bool isActive()
    {
        return map.activeSelf;
    }

    public void ChangeState(bool state)
    {

        currentMapObject.SetActive(state);
        map.SetActive(state);
        var pl = GameObject.Find("LocalPlayer").GetComponent<ListenersManager>();
        if (state)
        {
            pl.TabListeners.Add(this);
            pl.EscListeners.Add(this);
        }
        else
        {
            pl.TabListeners.Remove(this);
            pl.EscListeners.Remove(this);
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
        if (needMap)
        {
            var newButton = Instantiate(mapButton, new Vector3(currentMiniGameObject.transform.position.x+130, currentMiniGameObject.transform.position.y, currentMiniGameObject.transform.position.z), Quaternion.identity, currentMiniGameObject.transform);
            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate { ChooseMap(newMenu); });
        }

        Button btnPlane = currentMiniGameObject.GetComponent<Button>();
        btnPlane.onClick.AddListener(delegate { ChangeTarget(currentMiniGameObject.GetComponent<ButtonInfo>()); });

        newMenu.secondButton2 = secondButton;
        newMenu.menues = menues;
        newMenu.mapScript = mapS;
        newMenu.mainButton = currentMiniGameObject.GetComponent<ButtonInfo>();
        newMenu.mainButton.targetTransform = target;
        newMenu.mainButton.ownMenu = newMenu;
        if (menues.Count == 0) { targetMenu = newMenu; targetMenu.thisMenuTarget = true; targetMenu.currentTarget = target; newMenu.mainButton.ChangeTarget(true); newMenu.targetButton = newMenu.mainButton; }//Делаем новое меню таргетным
        newMenu.mainButton.text = name;

        newMenu.mainButton.buttonObject = currentMiniGameObject;
        menues.Add(newMenu);
        currentMiniGameObject.GetComponent<TextMeshProUGUI>().text = name;
        return newMenu;
    }

    public void ChangeTargetNum(Menu m, int n)
    {
        print(n);
        ChangeTarget(m.ALLminiButtons[n]);
    }
    public void ChangeTarget(ButtonInfo button)
    {
        if (!button.canChange) return;
        if (button.isTarget)
        {
            button.ChangeTarget(false);

            button.ownMenu.thisMenuTarget = false;
            button.ownMenu.targetButton = null;
            targetMenu = null;
        }
        else
        {
            if (targetMenu != null)
            {
                targetMenu.thisMenuTarget = false;
                targetMenu.targetButton.ChangeTarget(false);
            }

            button.ChangeTarget(true);
            button.ownMenu.thisMenuTarget = true;
            targetMenu = button.ownMenu;
            targetMenu.targetButton = button;
        }
    }

    public void RemoveMenu(Menu needRemove)
    {
        int num = menues.IndexOf(needRemove);
        for (var i = num + 1; i < menues.Count; i++)
        {
            menues[i].mainButton.buttonObject.transform.position = new Vector3(menues[i].mainButton.buttonObject.transform.position.x, menues[i].mainButton.buttonObject.transform.position.y + menues[num].heightVal, menues[i].mainButton.buttonObject.transform.position.z);
        }
        menues.RemoveAt(num);
        if (needRemove.thisMenuTarget) 
        {
            if (menues.Count == 0)
                { targetMenu = null; }
            else if (menues[0].miniButtons.Count > 0)
                ChangeTarget(menues[0].miniButtons[0]);
            else
                ChangeTarget(menues[0].mainButton);
        }
        Destroy(needRemove.mainButton.buttonObject);
    }
}