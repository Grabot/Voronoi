using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class HalfEdge
{
    public Vertex Origin;

    public HalfEdge Twin;
    public HalfEdge Next;
    public HalfEdge Prev;
}

