using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Состояние квеста */
public enum QuestState//Пронумерованый в порядке приоритетности
{
    Available = 0,
    Unavailable = 1, 
    Done = 2
}

/** Варианты таска */
public enum TaskType
{
    Cooking = 0, 
    Digging = 1,
    Standing = 2,
    Sleeping = 3
}