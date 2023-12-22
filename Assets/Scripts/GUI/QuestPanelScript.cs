using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestPanelScript : MonoBehaviourExtension
{
    public static QuestPanelScript Instance;
    [SerializeField] private GameObject questButtonPrefab;
    [SerializeField] private GameObject questMapButtonPrefab;
    public List<QuestButton> questButtons = new();
    public ButtonInfo targetQuestButton;

    void Awake()
    {
        Instance = this;
    }

    public void ChangeQuestButtonState(QuestController quest, bool newState)
    {
        questButtons.FindAll(button => ReferenceEquals(button.questStep, quest))
            .ForEach(button => button.ChangeTarget(newState));
    }

    public void ChangeStepButtonState(QuestStep step, bool newState)
    {
        questButtons.SelectMany(questButton => questButton.miniButtons)
            .ToList()
            .FindAll(button => (QuestStep)button.questStep == step)
            .ForEach(button => button.ChangeTarget(newState));
    }

    private const float StartYPosition = -70;
    private const float DifYPosition = 40;

    /** Переставить все кнопки */
    public void ResetButtons()
    {
        var yPos = StartYPosition;
        foreach (var questButton in questButtons)
        {
            questButton.transform.position = transform.position + new Vector3(-30, yPos, 0);
            yPos -= DifYPosition;

            if (questButton.hided) continue;
            foreach (var stepButton in questButton.miniButtons)
            {
                stepButton.transform.position = transform.position + new Vector3(-30, yPos, 0);
                yPos -= DifYPosition;
            }
        }
    }

    public QuestButton AddQuestButton(QuestController quest, bool needMap = false)
    {
        var newQuestButtonObject = Instantiate(questButtonPrefab, Vector3.zero, Quaternion.identity, transform);

        var newQuestButton = newQuestButtonObject.GetComponent<QuestButton>();
        newQuestButton.ChangeButtonTarget(quest);
        if (needMap) //Кнопка открытия графа
        {
            var newMapButtonObject = Instantiate(questMapButtonPrefab, new Vector3(130, 0 ,0), Quaternion.identity,
                newQuestButtonObject.transform);
            var newMapButton = newMapButtonObject.GetComponent<Button>();
            newMapButton.onClick.AddListener(delegate { MapScript.Instance.OpenWithQuest(quest); });
        }

        var btnPlane = newQuestButtonObject.GetComponent<Button>();
        btnPlane.onClick.AddListener(delegate { taskManager.UpdateTargetQuest(quest); });

        newQuestButton.menues = questButtons;
        newQuestButton.ownQuestButton = newQuestButton;
        if (questButtons.Count == 0)
        {
            targetQuestButton = newQuestButton;
            newQuestButton.ChangeTarget(true);
        } //Делаем новое меню таргетным

        newQuestButton.text = quest.currentQuestName;

        newQuestButton.buttonObject = newQuestButtonObject;
        questButtons.Add(newQuestButton);
        newQuestButtonObject.GetComponent<TextMeshProUGUI>().text = quest.currentQuestName;
        ResetButtons();
        return newQuestButton;
    }

    public void RemoveQuestButton(QuestButton needRemove)
    {
        questButtons.Remove(needRemove);
        targetQuestButton = null;
        /*if (needRemove == targetQuestButton)
        {
            if (questButtons.Count == 0)
                targetQuestButton = null;
            else if (questButtons[0].miniButtons.Count > 0)
                questButtons[0].miniButtons[0].ChangeTarget(true);
            else
                questButtons[0].ChangeTarget(true);
        }*/

        Destroy(needRemove.buttonObject);
        ResetButtons();
    }
}