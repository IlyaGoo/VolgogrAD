using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Circle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public int number;
    public string text;
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
        //but.colors = neww;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textObject.SetActive(false);
    }
}
