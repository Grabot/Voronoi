using UnityEngine;
using System.Collections;

public class Edge {

    Vertex m_v1;
    Vertex m_v2;

    public Edge(Vertex a_v1, Vertex a_v2)
    {
        m_v1 = a_v1;
        m_v2 = a_v2;
    }

    public Vertex v1
    {
        get { return m_v1; }
    }

    public Vertex v2
    {
        get { return m_v2; }
    }
}
