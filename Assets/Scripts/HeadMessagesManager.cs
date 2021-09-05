using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMessagesManager : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3();
    public StandartMoving mov;//Это не обязательно локальный Moving
    [SerializeField] GameObject HeadTextPrefab;
    public List<HeadMessage> massages = new List<HeadMessage>();

    public void AddMessege(string text, bool needDestroyPrevious = true)
    {
        if (needDestroyPrevious)
        {
            foreach (var messege in massages)
                messege.Close(false);
            massages.Clear();
        }

        var headText = Instantiate(HeadTextPrefab, transform);
        mov.dontBeReflect.Add(headText);
        headText.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
        headText.transform.localPosition = offset;//TODO поднимать сообщения все выше и выше
        var newTextComponent = headText.GetComponent<HeadMessage>();
        newTextComponent.SetText(text, this);
        massages.Add(newTextComponent);
    }
}
