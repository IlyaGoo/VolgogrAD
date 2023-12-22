using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Circle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //public TaskPoint taskPoint;
    private string text => step.stepName;
    [SerializeField] GameObject textPrefab;
    private GameObject textObject;
    public int level;
    public QuestStep step;

    private static readonly Color _enableColor = new Vector4(210 / 255f, 214 / 255f, 76 / 255f, 1);
    private static readonly Color _choosenColor = new Vector4(214 / 255f, 167 / 255f, 76 / 255f, 1);
    private static readonly Color _disableColor = new Vector4(214 / 255f, 83 / 255f, 76 / 255f, 1);
    private static readonly Color _endedColor = new Vector4(76 / 255f, 214 / 255f, 86 / 255f, 1);
    
    public void updateState()
    {
        Color newColor;
        switch (step.state)
        {
            case QuestState.Available:
                newColor = step.choosen ? _choosenColor : _enableColor;
                break;
            case QuestState.Unavailable:
                newColor = _disableColor;
                break;
            case QuestState.Done:
                newColor = _endedColor;
                break;
            default:
                newColor = new Color();
                break;
        }


        GetComponent<Image>().color = newColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textObject == null)
        {
            textObject = Instantiate(textPrefab,
                new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 40,
                    gameObject.transform.position.z), Quaternion.identity, gameObject.transform);
            textObject.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
        }
        else
        {
            textObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textObject.SetActive(false);
    }
}