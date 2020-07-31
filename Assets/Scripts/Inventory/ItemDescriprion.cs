using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriprion : MonoBehaviour {

    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

/*    private void Start() {
        title = transform.Find("Title").GetComponent<Text>();
        description = transform.Find("Desc").GetComponent<Text>();
        GameObject tmp = gameObject;
        //tmp.SetActive(false);
    }*/

    public void Set(string title, string description) {
        this.title.text = title;
        this.description.text = description;
    }

}
