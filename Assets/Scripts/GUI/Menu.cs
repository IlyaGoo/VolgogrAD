using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	// Use this for initialization

    public ButtonInfo mainButton;
    public TaskMenuScript menuScript;
    public List<Menu> menues;
    public bool hided = false;
    public List<ButtonInfo> miniButtons = new List<ButtonInfo>();
    public List<ButtonInfo> ALLminiButtons = new List<ButtonInfo>();
    public GameObject secondButton2;

    public bool thisMenuTarget = false;
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

    public ButtonInfo addButton(string text, Transform newTarget = null, int PointNum = -1)
    {
        var newMiniMenu = Instantiate(secondButton2, new Vector3(mainButton.buttonObject.transform.position.x, mainButton.buttonObject.transform.position.y - 50*(1+miniButtons.Count), 0), Quaternion.identity, mainButton.buttonObject.transform);
        var newMiniMenuInfo = newMiniMenu.GetComponent<ButtonInfo>();

        newMiniMenuInfo.numberCostil = costilNum;
        costilNum++;

        newMiniMenuInfo.text = text;
        newMiniMenuInfo.ChangePoint(PointNum);
        newMiniMenuInfo.ownMenu = this;
        newMiniMenuInfo.buttonObject = newMiniMenu;
        miniButtons.Add(newMiniMenuInfo);
        ALLminiButtons.Add(newMiniMenuInfo);
        heightVal += 50;
        newMiniMenu.GetComponent<TextMeshProUGUI>().text = text;
        newMiniMenuInfo.targetTransform = newTarget;

        Button btnPlane = newMiniMenu.GetComponent<Button>();
        btnPlane.onClick.AddListener(delegate { FindObjectOfType<TaskMenuScript>().ChangeTarget(newMiniMenuInfo); });

        if (miniButtons.Count == 1)
        {
            if (thisMenuTarget && mainButton.isTarget) { newMiniMenuInfo.ChangeTarget(true); currentTarget = newTarget; }
            mainButton.ChangeTarget(false); mainButton.canChange = false;
        }

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
            menues[i].mainButton.buttonObject.transform.position = new Vector3(menues[i].mainButton.buttonObject.transform.position.x, menues[i].mainButton.buttonObject.transform.position.y + 50, menues[i].mainButton.buttonObject.transform.position.z);
        }
        miniButtons.RemoveAt(num);
        if (button.isTarget)
            if(miniButtons.Count > 0) { miniButtons[0].ChangeTarget(true); currentTarget = miniButtons[0].targetTransform; targetButton = miniButtons[0]; }
            else { currentTarget = mainButton.targetTransform; mainButton.ChangeTarget(true); }
        Destroy(button.buttonObject);
    }
}
	

