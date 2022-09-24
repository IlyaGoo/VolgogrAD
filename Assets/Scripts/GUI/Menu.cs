using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : ButtonInfo {

	// Use this for initialization
    
    public TaskMenuScript menuScript;
    public List<Menu> menues;
    public bool hided = false;
    public List<ButtonInfo> miniButtons = new List<ButtonInfo>();
    public List<ButtonInfo> ALLminiButtons = new List<ButtonInfo>();
    public GameObject secondButton2;

    public bool thisMenuTarget => targetButton != null;
    public ButtonInfo targetButton;
    public Transform currentTarget;
    public MapScript mapScript;


    public float heightVal = 70;

    private int costilNum = 0; //Костыль для связи с картой

    public void DestroyOwnObject()
    {
        Destroy(gameObject);
    }

    public void HideMenu()
    {
        foreach (var buton in miniButtons)
        {
            buton.buttonObject.SetActive(false);
        }
    }

    public ButtonInfo addButton(string text, Transform newTarget = null, QuestStep gameController = null)
    {
        var newMiniMenu = Instantiate(secondButton2, new Vector3(buttonObject.transform.position.x, buttonObject.transform.position.y - 50*(1+miniButtons.Count), 0), Quaternion.identity, buttonObject.transform);
        var newMiniMenuInfo = newMiniMenu.GetComponent<ButtonInfo>();

        newMiniMenuInfo.gameController = gameController;
        costilNum++;

        newMiniMenuInfo.text = text;
        newMiniMenuInfo.ChangePoint(gameController);
        newMiniMenuInfo.ownMenu = this;
        newMiniMenuInfo.buttonObject = newMiniMenu;
        miniButtons.Add(newMiniMenuInfo);
        ALLminiButtons.Add(newMiniMenuInfo);
        heightVal += 50;
        newMiniMenu.GetComponent<TextMeshProUGUI>().text = text;
        newMiniMenuInfo.targetTransform = newTarget;

        Button btnPlane = newMiniMenu.GetComponent<Button>();
        btnPlane.onClick.AddListener(
            delegate { menuScript.ChangeTargerInterface(newMiniMenuInfo); });
            //delegate { FindObjectOfType<TaskMenuScript>().ChangeTarget(newMiniMenuInfo); });//todo
        return newMiniMenuInfo;
    }

    public void removeButton(ButtonInfo button)
    {
        if (button == null) return;
        int num = miniButtons.IndexOf(button);
        heightVal -= 50;
        for (var i = num + 1; i < miniButtons.Count; i++)
        {
            miniButtons[i].buttonObject.transform.position = new Vector3(miniButtons[i].buttonObject.transform.position.x, miniButtons[i].buttonObject.transform.position.y + 50, miniButtons[i].buttonObject.transform.position.z);
        }
        for (var i = menues.IndexOf(this) + 1; i < menues.Count; i++)
        {
            menues[i].buttonObject.transform.position = new Vector3(menues[i].buttonObject.transform.position.x, menues[i].buttonObject.transform.position.y + 50, menues[i].buttonObject.transform.position.z);
        }
        miniButtons.RemoveAt(num);
        if (button.isTarget)
        {
            // gameController._taskController.ChangeTarget(button.gameController.taskPoint);
            // if (miniButtons.Count > 0)
            // {
            //     gameController._taskController.ChangeTarget(miniButtons[0].gameController.taskPoint);
            // }
            // else
            // {
            //     ChangeTarget(true);
            // }
        }

        Destroy(button.buttonObject);
    }
}
	

