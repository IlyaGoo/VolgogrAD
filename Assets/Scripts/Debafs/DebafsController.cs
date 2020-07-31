using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class DebafsController : MonoBehaviour
{
    [SerializeField] GameObject[] debafsPrefabs;
    [SerializeField] TaskManager manager;
    public DescriptionController descController;
    public List<Debaf> currentDebafs = new List<Debaf>();
    int showCount = 0;
    readonly float offset = 50;

    public GameObject Player => manager.LocalPlayer;

    public Debaf AddDebaf(int num, bool ce = true)
    {
        foreach (var deb in currentDebafs)
        {
            if (deb.num == num)
            {
                deb.Reinit();
                return deb;
            }
        }
        var newDebaf = Instantiate(debafsPrefabs[num], transform);
        var dComponent = newDebaf.GetComponent<Debaf>();
        if (dComponent.needShow)
        {
            newDebaf.transform.position = transform.position + new Vector3(offset * showCount, 0, 0);
            showCount++;
        }
        currentDebafs.Add(dComponent);
        dComponent.On(Player, -1, this, ce);
        return dComponent;
    }

    public void RemoveDebaf(Debaf removingDebaf)
    {
        var i = currentDebafs.IndexOf(removingDebaf);
        if (i == -1) return;//может возникнуть, например, когда мы спали командой, а не по-настоящему
        if (removingDebaf.needShow)
        {
            showCount--;
            for (var j = i + 1; j < currentDebafs.Count; j++)
                currentDebafs[j].transform.position -= new Vector3(offset, 0, 0);
        }
        currentDebafs.RemoveAt(i);
    }
}

public enum BafType
{
    Possitive, Negative, Neutral
}
