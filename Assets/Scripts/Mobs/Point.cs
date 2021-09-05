using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Point : MonoBehaviour {

    public Point[] Points;
    public List<GraphEdge> Edges = new List<GraphEdge>();
    public bool isMain = true;
    public int number;

    public bool IsUnvisited = true;
    public float EdgesWeightSum = float.MaxValue;
    public Point PreviousVertex = null;

    public bool botsCanStay;

    public void AddEdge(GraphEdge newEdge)
    {
        Edges.Add(newEdge);
    }

    public void AddStayMob(MobController mob)
    {
        var dialogue = GetComponent<MobDialogue>();
        if (dialogue == null)
            dialogue = gameObject.AddComponent<MobDialogue>();

        dialogue.AddStayMob(mob);
    }

    public void RemoveStayMob(MobController mob)
    {
        var dialogue = GetComponent<MobDialogue>();
        if (dialogue == null) return;
        dialogue.RemoveStayMob(mob);
        if (dialogue.Empty)
            Destroy(dialogue);
    }
}
