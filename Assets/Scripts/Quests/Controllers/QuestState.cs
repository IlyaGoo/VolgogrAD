using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Состояние квеста */
public enum QuestState//Пронумерованый в порядке приоритетности
{
    Active = 0,
    Available = 1,
    Unavailable = 2, 
    Done = 3
}

/** Варианты таска */
public enum TaskType
{
    Cooking = 0, 
    Digging = 1,
    Standing = 2,
    Sleeping = 3
}