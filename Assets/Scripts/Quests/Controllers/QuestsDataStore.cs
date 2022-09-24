using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/** Временное хранилище для рецептов и других заданий */
public static class QuestsDataStore
{
    #region tushnyak
    public static QuestInfo Tushnyak => new QuestInfo(new[] {
            new QuestStepInfo(
                false,
                3,
                true,
                "Нарезать лук и морковь", 
                false, 
                "qe_game",
                (int)CampObjectType.Kitchen,
                new List<int>()
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Смешать сосус", 
                false, 
                "qe_game",
                (int)CampObjectType.Kitchen,
                new List<int>()
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Засыпать гречу", 
                false, 
                "fast_e_game",
                (int)CampObjectType.Campfire,
                new List<int>()
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Обжарить лук", 
                false, 
                "qe_game",
                (int)CampObjectType.Kitchen,
                new List<int>(0)
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Обжарить тушенку", 
                false, 
                "qe_game",
                (int)CampObjectType.Kitchen,
                new List<int>()
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Слить воду", 
                false, 
                "fast_e_game",
                (int)CampObjectType.Campfire,
                new List<int>{2}
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Добивить смесь", 
                false, 
                "fast_e_game",
                (int)CampObjectType.Campfire,
                new List<int>{1, 3}
            ),
            new QuestStepInfo(
                false,
                3,
                true,
                "Добавить и приготовить", 
                false, 
                "fast_e_game",
                (int)CampObjectType.Campfire,
                new List<int>{4, 5, 6}
            )
        }, "Греча с тушонкой"
    );
    #endregion

    public static QuestInfo Rascop => new QuestInfo(new[] {
            new QuestStepInfo(
                false,
                1,
                false,
                "Придти на костыльное плато", 
                true, 
                "todo",
                (int)CampObjectType.MainPoint,
                new List<int>()
            )
        }, "Раскоп"
    );

    public static QuestInfo Postr => new QuestInfo(new[] {
            new QuestStepInfo(
                true,
                1,
                false,
                "", 
                true, 
                "todo",
                (int)CampObjectType.MainPoint,
                new List<int>()
            )
        }, "Построение"
    );
}
