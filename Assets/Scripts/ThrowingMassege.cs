using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThrowingMassege : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void Close()
    {
        Destroy(gameObject);
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }
}
