using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Algo : MonoBehaviour {
    public static Algo instance;
    public List<Point> points;
    public List<Point>[,] paths;

    public List<GraphEdge> GrEdges = new List<GraphEdge>();

    private Algo()
    { }

    private void Awake()
    {
        instance = this;
    }

    void Start () {
        points = new List<Point>(gameObject.transform.childCount);
        paths = new List<Point>[gameObject.transform.childCount, gameObject.transform.childCount];
        //points = gameObject.transform
        foreach (Transform e in gameObject.transform)
        {
            var pointElement = e.GetComponent<Point>();
            pointElement.number = points.Count;
            points.Add(pointElement);
            foreach (var el in pointElement.Points)
            {
                 if (points.Contains(el)) continue;
                 var newEdge = new GraphEdge(new Point[2] { pointElement, el });
                pointElement.Edges.Add(newEdge);
                el.Edges.Add(newEdge);
            }
        }
    }

    public Point FindUnvisitedVertexWithMinSum()
    {
        var minValue = float.MaxValue;
        Point minVertexInfo = null;
        foreach (var i in points)
        {
            if (i.IsUnvisited && i.EdgesWeightSum < minValue)
            {
                minVertexInfo = i;
                minValue = i.EdgesWeightSum;
            }
        }
        return minVertexInfo;
    }

    public List<Point> FindShortestPath(int startVertexNum, int finishVertexNum)
    {
        var res = paths[startVertexNum, finishVertexNum];
        if (res != null)
            return res;

        var startVertex = points[startVertexNum];
        var finishVertex = points[finishVertexNum];
        startVertex.EdgesWeightSum = 0;
        while (true)
        {
            var current = FindUnvisitedVertexWithMinSum();
            if (current == null)
            {
                break;
            }

            SetSumToNextVertex(current);
        }

        for(var i = 0; i < points.Count; i++)
        {
            var r = GetPath(startVertex, points[i]);
            paths[i, startVertexNum] = r;
            var r2 = new List<Point>(r);
            r2.Reverse();
            paths[startVertexNum, i] = r2;
        }

        //List<Point> result = GetPath(startVertex, finishVertex);
        RestartPoints();
        //result.Reverse();
        return paths[startVertexNum, finishVertexNum];
    }

    void RestartPoints()
    {
        foreach(var p in points)
        {
            p.IsUnvisited = true;
            p.EdgesWeightSum = float.MaxValue;
            p.PreviousVertex = null;
        }
    }

    List<Point> GetPath(Point startVertex, Point endVertex)
    {
        var pointList = new List<Point>
        {
            endVertex
        };
        var a = endVertex;
        while (startVertex.number != a.number)
        {
            a = a.PreviousVertex;
            pointList.Add(a);
        }
        return pointList;
    }

    void SetSumToNextVertex(Point info)
    {
        info.IsUnvisited = false;
        foreach (var e in info.Edges)
        {
            var nextInfo = e.GetPath(info);
            var sum = info.EdgesWeightSum + e.EdgeWeight;
            if (sum < nextInfo.EdgesWeightSum)
            {
                nextInfo.EdgesWeightSum = sum;
                nextInfo.PreviousVertex = info;
            }
        }
    }

    public class GraphPoint
    {
        public bool IsUnvisited; //не посещена
        public Point point;
        public float EdgesWeightSum;
        public GraphPoint PreviousVertex;
        public List<GraphEdge> Edges;

        public GraphPoint(Point pointData)
        {
            Edges = new List<GraphEdge>();
            point = pointData;
            IsUnvisited = true;
            EdgesWeightSum = float.MaxValue;
            PreviousVertex = null;
        }

        public void AddEdge(GraphEdge newEdge)
        {
            Edges.Add(newEdge);
        }
    }



}

public class GraphEdge
{
    public Point[] ConnectedVertex = new Point[2];
    public Point[] AllPoints;

    public float EdgeWeight;

    public GraphEdge(Point[] connectedVertexs)
    {
        AllPoints = connectedVertexs;
        ConnectedVertex = new Point[2]{ connectedVertexs[0], connectedVertexs[connectedVertexs.Length - 1]};
        EdgeWeight = Vector3.Distance(connectedVertexs[0].gameObject.transform.position, connectedVertexs[1].gameObject.transform.position);
    }

    public Point GetPath(Point startPoint)
    {
        if (ConnectedVertex[0] == startPoint)
        {
            return ConnectedVertex[1];
        }
        else
        {
            return ConnectedVertex[0];
        }
    }
}
