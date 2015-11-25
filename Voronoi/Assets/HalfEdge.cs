using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HalfEdge
{
    private Vertex origin;

    public HalfEdge(Vertex v)
    {
        origin = new Vertex(v.x, v.y);

        Twin = this;
        Next = null;
        Prev = null;
    }

    public Vertex Origin
    {
        get { return origin; }
        set { origin = new Vertex(value.x, value.y); }
    }

    public Triangle Face;
    public HalfEdge Twin;
    public HalfEdge Next;
    public HalfEdge Prev;
}

