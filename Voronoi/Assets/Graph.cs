using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour
{
    public float m_Radius = 0.5f;
    private List<Vertex> vertices = new List<Vertex>();
    private List<HalfEdge> halfEdges = new List<HalfEdge>();
    
    private void Start()
    {
        // Create corner vertex
        Vertex v1 = new Vertex(-5, -5);
        Vertex v2 = new Vertex(5, -5);
        Vertex v3 = new Vertex(5, 5);
        Vertex v4 = new Vertex(-5, 5);

        // Create halfedges
        HalfEdge h1 = new HalfEdge(v1);
        HalfEdge h2 = new HalfEdge(v2);
        HalfEdge h3 = new HalfEdge(v4);

        h1.Next = h2;
        h2.Next = h3;
        h3.Next = h1;

        // Create halfedges
        HalfEdge h4 = new HalfEdge(v4);
        HalfEdge h5 = new HalfEdge(v2);
        HalfEdge h6 = new HalfEdge(v3);
        h2.Twin = h4;

        h4.Next = h5;
        h5.Next = h6;
        h6.Next = h4;

        halfEdges.Add(h1);
        halfEdges.Add(h2);
        halfEdges.Add(h3);
        halfEdges.Add(h4);
        halfEdges.Add(h5);
        halfEdges.Add(h6);

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        AddVertex(h1, new Vertex(-2, -2));

    }

    private void AddVertex(HalfEdge h, Vertex v)
    {
        HalfEdge h1 = h;
        HalfEdge h2 = h.Next;
        HalfEdge h3 = h.Next.Next;

        HalfEdge h4 = new HalfEdge(v);
        HalfEdge h5 = new HalfEdge(h1.Origin);
        HalfEdge h6 = new HalfEdge(v);
        HalfEdge h7 = new HalfEdge(h2.Origin);
        HalfEdge h8 = new HalfEdge(v);
        HalfEdge h9 = new HalfEdge(h3.Origin);

        // Set all twins
        h4.Twin = h5;
        h5.Twin = h4;
        h6.Twin = h7;
        h7.Twin = h6;
        h8.Twin = h9;
        h9.Twin = h8;

        // Set all next
        h1.Next = h6;
        h6.Next = h5;
        h5.Next = h1;

        h2.Next = h8;
        h8.Next = h7;
        h7.Next = h2;

        h3.Next = h4;
        h4.Next = h9;
        h9.Next = h3;
    }

    List<Vector2> Q = new List<Vector2>();
    List<Vector2> T = new List<Vector2>();
    List<Vector2> S = new List<Vector2>();
    public void OnRenderObject()
    {
        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        // Vertex colors change from red to green
        GL.Color(new Color(0, 0, 0));

        foreach (HalfEdge halfEdge in halfEdges)
        {
            GL.Vertex3(halfEdge.Origin.Position.x, 0, halfEdge.Origin.Position.y);
            GL.Vertex3(halfEdge.Next.Origin.Position.x, 0, halfEdge.Next.Origin.Position.y);
        }
        
        GL.End();
        GL.PopMatrix();
    }

    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            Vector3 newPos = hits[0].point;
            vertices.Add(new Vertex(newPos));

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);
        }
    }
}
