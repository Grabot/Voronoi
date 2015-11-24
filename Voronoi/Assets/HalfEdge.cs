using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HalfEdge
{
    public HalfEdge(Vertex v)
    {
        Origin = v;
        Twin = null;
        Next = null;
        Prev = null;
    }

    public Vertex Origin;
    public HalfEdge Twin;
    public HalfEdge Next;
    public HalfEdge Prev;
}

