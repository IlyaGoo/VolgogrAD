using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleCampAreaDoing : SleepAreaDoing
{
    protected override string[] GetLabelTexts() => new string[2] { "Войти", "Выйти" };
}
