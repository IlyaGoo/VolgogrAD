using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GetMusicLength : MonoBehaviour
{
    [SerializeField]
    private AudioClip music;

    public void OnValidate()
    {
        if (music != null)
            print(music.length);
    }
}
