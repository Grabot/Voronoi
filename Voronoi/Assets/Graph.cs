using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Graph : MonoBehaviour
{

    private SanderGraph sander;
    private void Start()
    {
        sander = new SanderGraph();
        sander.createGraph();
    }

    
    public void OnRenderObject()
    {
        sander.CreateLineMaterial();
        // Apply the line material
        sander.lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        // Vertex colors change from red to green
        GL.Color(new Color(1, 0, 0));
        
        foreach (HalfEdge halfEdge in sander.HalfEdges)
        {
            GL.Vertex3(halfEdge.Origin.x, 0, halfEdge.Origin.y);
            GL.Vertex3(halfEdge.Next.Origin.x, 0, halfEdge.Next.Origin.y);
        }
        
        GL.Begin(GL.LINES);
        float heading = 0;
        float extra = (360 / 100);
        for (int a = 0; a < (360+ extra); a += 360 / 100)
        {
            GL.Vertex3(Mathf.Cos(heading) * 6, 0, Mathf.Sin(heading) * 6);
            heading = a * Mathf.PI / 180;
            GL.Vertex3(Mathf.Cos(heading) * 6, 0, Mathf.Sin(heading) * 6);
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
            Vertex me = new Vertex(newPos.x, newPos.y);
            sander.AddVertex(me);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
