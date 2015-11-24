using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour
{
    public float m_Radius = 0.5f;
    private List<Vertex> vertices = new List<Vertex>();
    private List<HalfEdge> halfEdges = new List<HalfEdge>();
    private List<Triangle> triangles = new List<Triangle>();
    
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

        triangles.Add( new Triangle(v1, v2, v4) );

        h1.Next = h2;
        h2.Next = h3;
        h3.Next = h1;

        // Create halfedges
        HalfEdge h4 = new HalfEdge(v4);
        HalfEdge h5 = new HalfEdge(v2);
        HalfEdge h6 = new HalfEdge(v3);

        triangles.Add(new Triangle(v4, v2, v3));

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
            Vertex me = new Vertex(newPos);
            vertices.Add(me);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);

            foreach (Triangle t in triangles)
            {
                Debug.Log("inside Triangle: " + t.inside(me));
            }
        }
    }
}
