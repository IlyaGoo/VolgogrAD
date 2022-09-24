using UnityEngine;

/** Скрипт, висящий на объекте головы сущности во время сна */
public class SleeperHead : MonoBehaviour
{
    public AbstractEntityInfo OwnerInfo;
    
    public void Init(AbstractEntityInfo entityInfo)
    {
        OwnerInfo = entityInfo;
        var spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + entityInfo.GetHeadNum);
        GetComponent<SpriteRenderer>().sprite = spritesHead[3];
    }

    public void SleeperWakeUp()
    {
        Destroy(gameObject);
    }
}
