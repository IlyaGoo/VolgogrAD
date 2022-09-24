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
    public readonly GameObject EntityObject;
    public readonly string Identity;

    protected AbstractEntityInfo(GameObject obj, string id)
    {
        EntityObject = obj;
        Identity = id;
    }

    public virtual bool IsDisconnected { get; set; }
    public virtual int GetHeadNum { get;}
    public virtual int GetBodyNum { get;}
    public virtual int GetLegsNum { get;}
    
    public override bool Equals(object obj) => Equals(obj as AbstractEntityInfo);

    private bool Equals(AbstractEntityInfo p)
    {
        if (p is null) return false;
        if (ReferenceEquals(this, p)) return true;
        if (GetType() != p.GetType()) return false;
        return Identity == p.Identity;
    }

    public override int GetHashCode() => Identity.GetHashCode();

    public static bool operator ==(AbstractEntityInfo lhs, AbstractEntityInfo rhs)
    {
        if (!(lhs is null)) return lhs.Equals(rhs);
        return rhs is null;
    }

    public static bool operator !=(AbstractEntityInfo lhs, AbstractEntityInfo rhs) => !(lhs == rhs);
}
