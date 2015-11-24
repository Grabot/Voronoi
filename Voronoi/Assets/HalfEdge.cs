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

        color = Color.black;
    }

    public Color color;

    public Vertex Origin;
    public HalfEdge Twin;
    public HalfEdge Next;
    public HalfEdge Prev;
}

