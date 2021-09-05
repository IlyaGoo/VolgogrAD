using UnityEngine;

/** Класс, хранящий в себе основную информацию о сущностях
 * Создан для случаев, когда игрок резко отключился, но мы должны обратиться к его полям
 * Так как мы не можем контролировать уничтожение объекта игрока, нам необходим
 * этот абсорбатор основной информации о нем
 * Как пример: если игрок ливнул во время сна, нам нужно удалить его из спальника, убрать спрайт его головы
 * Наследние BotInfo необходим для полиморфизма, чтобы мы рассматривали ботов равноценно с игроками и
 * по идее является просто интерфейсом, ссылающимся на поля существующего бота
 **/
public abstract class AbstractEntityInfo
{
    public GameObject entityObject;
    public string identity;

    public AbstractEntityInfo(GameObject obj, string id)
    {
        entityObject = obj;
        identity = id;
    }

    public virtual bool IsDisconnected { get; set; }
    public virtual int GetHeadNum { get;}
    public virtual int GetBodyNum { get;}
    public virtual int GetLegsNum { get;}
}
