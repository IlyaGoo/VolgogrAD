using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Класс-оболочка для данных, которые потребуются для зоны начала квеста
Вешается на объект лагеря типа костра или кухни
Когда начинается квест на кухне(например), у кухни спавнится подобъект-триггер начала квеста,
который обращается к этому скрипту и стягивает отсюда данные
 */
public class QuestStartAreaData : MonoBehaviour
{
    public string labelText = "";
    public Vector3 textOffset = Vector3.zero;
    public int labelSize = 0;

}
