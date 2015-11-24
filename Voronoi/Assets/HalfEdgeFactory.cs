using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HalfEdgeFactory
{
    public static List<HalfEdge> Create(Vertex v1, Vertex v2)
    {
        List<HalfEdge> halfEdges = new List<HalfEdge>();

        HalfEdge h1 = new HalfEdge(v1);
        HalfEdge h2 = new HalfEdge(v2);

        h1.Twin = h2;
        h2.Twin = h1;

        halfEdges.Add(h1);
        halfEdges.Add(h2);

        return halfEdges;
    }

}

