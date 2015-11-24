using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour
{
    private Distance m_distance = new Euclidian();
    public float m_Radius = 5f;
    List<Vector2> vertices = new List<Vector2>();
    private GameObject m_GameObject;

    void Awake()
    {
        createVertices();
    }

    private void createVertices()
    {
        vertices.Add(new Vector2(3, 3));
        GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_GameObject = this.gameObject;
        gob.transform.position = vertices[0];
        gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);
            
    }

    List<Vector2> Q = new List<Vector2>();
    List<Vector2> T = new List<Vector2>();
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

        Q = vertices.OrderBy(v => v.y).ToList();
        foreach (Vector2 e in Q)
        {
            foreach (Vector2 s in T)
            {
                GL.Vertex3(e.x, 0, e.y);
                GL.Vertex3(s.x, 0, s.y);
            }
            T.Add(e);
        }

        T.Clear();
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
            vertices.Add(newPos);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);

            DoTriangulation();
        }
    }
    
    private void DoTriangulation()
    {
        Q = vertices.OrderBy(v => v.y).ToList();
        foreach (Vector2 e in Q)
        {
            foreach (Vector2 s in T)
            {
                Debug.Log(m_distance.calculate(e, s));
            }
            T.Add(e);
        }

        T.Clear();
    }
}
