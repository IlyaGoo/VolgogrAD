using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour {

    public Point[] Points;
    public List<GraphEdge> Edges = new List<GraphEdge>();
    public bool isMain = true;
    public int number;

    public bool IsUnvisited = true;
    public float EdgesWeightSum = float.MaxValue;
    public Point PreviousVertex = null;

    public void AddEdge(GraphEdge newEdge)
    {
        Edges.Add(newEdge);
    }
}
