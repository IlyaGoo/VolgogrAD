using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Circle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public TaskPoint taskPoint;
    string text => taskPoint.pointData.name;
    [SerializeField] GameObject textPrefab;
    private GameObject textObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textObject == null)
        {
            textObject = Instantiate(textPrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 40, gameObject.transform.position.z), Quaternion.identity, gameObject.transform);
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
