using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** Класс предоставляющий объект, на котором мы должны заспавнить зону
для квестов, в которые еще необходимо вписаться, чтобы начать
Например: готовка
 */
public class StarQuestObjectGetter : IStarQuestObjectGetter
{
    CampObjectType objectType;

    public StarQuestObjectGetter(CampObjectType needObjectType){
        objectType = needObjectType;
    }

    public GameObject GetObject() {
        return CampObject.AllCampObjects.Find(obj => obj.objectType == objectType).gameObject;
    }
}

public interface IStarQuestObjectGetter
{
    public GameObject GetObject();
}
