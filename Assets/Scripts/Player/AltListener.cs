using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AltListener : MonoBehaviour, IListener, IUpListener
{
    List<IAltLabelShower> labels = new List<IAltLabelShower>();
    GameObject panelPrefab;
    bool needShow;

    Vector3 normalScale = new Vector3(10, 2, 1);
    Vector3 mirrorScale = new Vector3(-10, 2, 1);

    public void EventDid()
    {
        if (labels.Count > 0)
            EventUpDid();
        needShow = true;
    }

    public void EventUpDid()
    {
        needShow = false;
        foreach(var label in labels)
        {
            label.Panel = null;
        }
        labels.Clear();
    }

    void Update()
    {
        if (needShow)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 8);
            IAltLabelShower shower;
            foreach (var hit in hitColliders)
            {
                if ((shower = hit.GetComponent<IAltLabelShower>()) != null)
                {
                    if (labels.Contains(shower)) continue;
                    labels.Add(shower);

                    shower.Panel = Instantiate(panelPrefab, shower.TranformForPanel.position + shower.Offset, Quaternion.identity, shower.TranformForPanel);
                    shower.Panel.transform.localScale = shower.TranformForPanel.localScale.x > 0 ? normalScale : mirrorScale;
                    var meshComp = shower.Panel.GetComponentInChildren<TextMeshPro>();
                    shower.ShowLabel(gameObject);
                    meshComp.text = shower.LabelName;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        panelPrefab = (GameObject)Resources.Load("AltPanel");
        GetComponent<ListenersManager>().AltListeners.Add(this);
        GetComponent<ListenersManager>().AltUpListeners.Add(this);
    }

}

public interface IAltLabelShower
{
    Vector3 Offset { get; }

    void ShowLabel(GameObject player);
    string LabelName { get; set; }
    Transform TranformForPanel { get; set; }
    GameObject Panel { get; set; }
}
