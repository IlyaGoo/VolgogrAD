using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class DescriptionController : MonoBehaviourExtension
{
    public static DescriptionController instance;
    public GameObject DescriptionRef;
    static float delta;
    static Vector3 offset;
    public GameObject cuurentHoverDescription;

    private DescriptionController()
    { }

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (DescriptionRef.activeSelf)
        {
            if (Input.mousePosition.y > Screen.height / 2)
                offset = new Vector3(180, -(50 + delta) / 2 + 10);
            else
                offset = new Vector3(180, (50 + delta) / 2 - 10);

            DescriptionRef.GetComponent<RectTransform>().position = Input.mousePosition + offset;
        }
    }

    public void ShowDescription(HoverCell hov)
    {
        SetDescription(hov.gameObject, hov.CellRef.Content.Title, hov.CellRef.Content.Description, hov.size);
        if (!hov.CellRef.CanInteract(localPlayer))
        {
            var OwnerRef = DescriptionRef.transform.Find("Owner");
            OwnerRef.gameObject.SetActive(true);
            var guiComponent = OwnerRef.GetComponent<TextMeshProUGUI>();
            guiComponent.text = "Владелец: " + hov.CellRef.Content.owner.GetComponent<IAltLabelShower>().LabelName;
            delta += guiComponent.preferredHeight;
            DescriptionRef.GetComponent<RectTransform>().sizeDelta = hov.size + new Vector3(0, delta);
        }
    }

    void SetDescription(GameObject obj, string title, string description, Vector3 size)
    {
        cuurentHoverDescription = obj;

        if (Input.mousePosition.y > Screen.height / 2)
            offset = new Vector3(180, -(50 + delta) / 2 + 10);
        else
            offset = new Vector3(180, (50 + delta) / 2 - 10);

        DescriptionRef.GetComponent<RectTransform>().position = Input.mousePosition + offset;
        DescriptionRef.GetComponent<ItemDescriprion>().Set(title, description);
        delta = DescriptionRef.transform.Find("Title").Find("Desc").GetComponent<TextMeshProUGUI>().preferredHeight;

        DescriptionRef.GetComponent<RectTransform>().sizeDelta = size + new Vector3(0, delta);
        DescriptionRef.SetActive(true);
    }

    public void ShowDescription(Debaf hov)
    {
        SetDescription(hov.gameObject, hov.debafName, hov.description, Debaf.size);
    }

    public void DestroyRef(GameObject hov, bool force = false)
    {
        if (hov != cuurentHoverDescription && !force) return;
        cuurentHoverDescription = null;
        if (DescriptionRef != null)
        {
            var OwnerRef = DescriptionRef.transform.Find("Owner");
            OwnerRef.gameObject.SetActive(false);
            DescriptionRef.SetActive(false);
            //Destroy(DescriptionRef);
        }
    }
}
