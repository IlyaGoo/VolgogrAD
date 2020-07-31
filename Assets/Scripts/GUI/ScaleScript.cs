using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleScript : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    [SerializeField] OptionsScript options;
    [SerializeField] int num;
    [SerializeField] RectTransform sprite;

    public void OnBeginDrag(PointerEventData eventData)
    {
        options.ClickOnButon(num, sprite.position.x - sprite.rect.width / 2);
    }

    public void OnDrag(PointerEventData eventData)
    {
        options.ClickOnButon(num, sprite.position.x - sprite.rect.width / 2);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        options.WriteInOptionsFile();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        options.ClickOnButon(num, sprite.position.x - sprite.rect.width / 2);
        options.WriteInOptionsFile();
    }
}
