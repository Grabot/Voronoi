using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Brute : MonoBehaviour
{
    public float m_Radius = 0.5f;
    private List<Vertex> vertices = new List<Vertex>();
    private List<Triangle> triangles = new List<Triangle>();
    private List<Edge> edges = new List<Edge>();

    private void Start()
    {
        // Create corner vertex
        Vertex v1 = new Vertex(-5, -5);
        Vertex v2 = new Vertex(5, -5);
        Vertex v3 = new Vertex(5, 5);
        Vertex v4 = new Vertex(-5, 5);

        triangles.Add(new Triangle(v1, v2, v4));
        edges.Add(new Edge(v1, v2));
        edges.Add(new Edge(v2, v4));
        edges.Add(new Edge(v4, v1));

        triangles.Add(new Triangle(v4, v2, v3));
        edges.Add(new Edge(v4, v2));
        edges.Add(new Edge(v2, v3));
        edges.Add(new Edge(v3, v4));

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

        GL.Color(new Color(1, 0, 0));

        foreach( Edge e in edges )
        {
            GL.Vertex3(e.v1.x, 0, e.v1.y);
            GL.Vertex3(e.v2.x, 0, e.v2.y);
        }
        /*
        foreach (HalfEdge halfEdge in halfEdges)
        {
            GL.Vertex3(halfEdge.Origin.Position.x, 0, halfEdge.Origin.Position.y);
            GL.Vertex3(halfEdge.Next.Origin.Position.x, 0, halfEdge.Next.Origin.Position.y);
        }
        */

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
            Vertex newPoint = new Vertex(newPos);
            vertices.Add(newPoint);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);

            foreach (Triangle t in triangles)
            {
                if( t.inside(newPoint))
                {
                    edges.Add(new Edge(t.getTriangle[0], newPoint));
                    edges.Add(new Edge(t.getTriangle[1], newPoint));
                    edges.Add(new Edge(t.getTriangle[2], newPoint));
                    triangles.Remove(t);
                    triangles.Add(new Triangle(t.getTriangle[0], t.getTriangle[1], newPoint));
                    triangles.Add(new Triangle(t.getTriangle[1], t.getTriangle[2], newPoint));
                    triangles.Add(new Triangle(t.getTriangle[2], t.getTriangle[0], newPoint));
                }
            }
        }
    }
}
