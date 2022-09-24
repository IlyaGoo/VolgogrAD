using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Minigame : MonoBehaviourExtension
{
    public QuestStep step;
    public bool startSpawn;
    [SerializeField] TextMeshPro _textMesh;

    public void ChangeName(string newName)
    {
        if (_textMesh != null)
            _textMesh.text = newName;
    }

    public void Finish()
    {
        step.MiniGameFinish();
    }
}
