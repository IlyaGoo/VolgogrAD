using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestButton : ButtonInfo {
    
    public List<QuestButton> menues;
    public bool hided;
    public List<ButtonInfo> miniButtons = new();
    [SerializeField] private GameObject questStepButtonPrefab;

    public void DestroyOwnObject()
    {
        Destroy(gameObject);
    }

    public void HideMenu()
    {
        foreach (var button in miniButtons)
        {
            button.buttonObject.SetActive(false);
        }
    }

    public void AddButton(QuestStep newQuestStep)
    {
        if (miniButtons.Exists(button => ReferenceEquals(button.questStep, newQuestStep)))
        {
            Debug.LogError("Попытка создать кнопку на шаг квеста, для которого кнопка уже есть");
        }
        
        var newStepObject = Instantiate(questStepButtonPrefab, Vector3.zero, Quaternion.identity, buttonObject.transform);
        var newMiniMenuInfo = newStepObject.GetComponent<ButtonInfo>();

        newMiniMenuInfo.questStep = newQuestStep;

        newMiniMenuInfo.text = newQuestStep.stepName;
        newMiniMenuInfo.ChangeButtonTarget(newQuestStep);
        newMiniMenuInfo.ownQuestButton = this;
        newMiniMenuInfo.buttonObject = newStepObject;
        miniButtons.Add(newMiniMenuInfo);
        newStepObject.GetComponent<TextMeshProUGUI>().text = newQuestStep.stepName;

        var btnPlane = newStepObject.GetComponent<Button>();
        btnPlane.onClick.AddListener(
            delegate { taskManager.UpdateTargetStep(newQuestStep); }
            );
        QuestPanelScript.Instance.ResetButtons();
    }

    public void RemoveButton(QuestStep step)
    {
        var button = miniButtons.Find(button => button.questStep == step);
        if (button == null) return;
        miniButtons.Remove(button);
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
        QuestPanelScript.Instance.ResetButtons();
    }
}
	

