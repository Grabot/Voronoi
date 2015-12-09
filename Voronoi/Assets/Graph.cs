using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Graph : MonoBehaviour
{
    private SanderGraph sander;
    private bool circleOn = false;
    private bool faceOn = false;
    private bool edgeson = false;
    private void Start()
    {
        sander = new SanderGraph();
        sander.createGraph();
    }

    private void drawEdges()
    {
        GL.Begin(GL.LINES);
        // Vertex colors change from red to green

        foreach (HalfEdge halfEdge in sander.HalfEdges)
        {
            GL.Vertex3(halfEdge.Origin.x, 0, halfEdge.Origin.y);
            GL.Vertex3(halfEdge.Next.Origin.x, 0, halfEdge.Next.Origin.y);
        }
        GL.End();
    }

    private void fillFaces()
    {
        GL.Begin(GL.TRIANGLES);

        foreach( Triangle face in sander.Faces )
        {
            GL.Color(face.m_colour);
            GL.Vertex3(face.HalfEdge.Origin.x, 0, face.HalfEdge.Origin.y);
            GL.Vertex3(face.HalfEdge.Next.Origin.x, 0, face.HalfEdge.Next.Origin.y);
            GL.Vertex3(face.HalfEdge.Prev.Origin.x, 0, face.HalfEdge.Prev.Origin.y);
        }


        GL.End();

    }

    private void drawCircles()
    {
        float radius = 0;
        GL.Begin(GL.LINES);

        //System.Random rand = new System.Random();

        //GL.Color(new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));

        foreach (Triangle face in sander.Faces )
        {
            GL.Color(face.m_colour);
            radius = face.Diameter();
            float heading = 0;
            float extra = (360 / 100);
            for (int a = 0; a < (360 + extra); a += 360 / 100)
            {
                //the circle.
                GL.Vertex3((Mathf.Cos(heading) * radius) + face.Circumcenter().x, 0, (Mathf.Sin(heading) * radius) + face.Circumcenter().y);
                heading = a * Mathf.PI / 180;
                GL.Vertex3((Mathf.Cos(heading) * radius) + face.Circumcenter().x, 0, (Mathf.Sin(heading) * radius) + face.Circumcenter().y);

                //midpoint of the circle.
                GL.Vertex3((Mathf.Cos(heading) * 0.1f) + face.Circumcenter().x, 0, (Mathf.Sin(heading) * 0.1f) + face.Circumcenter().y);
                GL.Vertex3((Mathf.Cos(heading) * 0.2f) + face.Circumcenter().x, 0, (Mathf.Sin(heading) * 0.2f) + face.Circumcenter().y);
            }
        }
        GL.End();
    }


    // Testing only
    public void DrawVoronoi()
    {
        GL.Begin(GL.LINES);
        Console.Out.WriteLine("Start");
        foreach (HalfEdge halfEdge in sander.HalfEdges)
        {
            if (halfEdge.Twin == null)
                continue;

            Triangle f1 = halfEdge.Face;
            Triangle f2 = halfEdge.Twin.Face;

            if (f1 is Triangle && f2 is Triangle)
            {
                Triangle t1 = f1 as Triangle;
                Triangle t2 = f2 as Triangle;

                Vertex v1 = t1.Circumcenter();
                Vertex v2 = t2.Circumcenter();

                GL.Vertex3(v1.x, 0, v1.y);
                GL.Vertex3(v2.x, 0, v2.y);
            }
        }
        GL.End();
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

        if (edgeson)
        {
            drawEdges();
        }

        if (faceOn)
        {
            fillFaces();
        }

        if (circleOn)
        {
            drawCircles();
        }

        DrawVoronoi();

        GL.PopMatrix();
    }

    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            circleOn = !circleOn;
        }

        if (Input.GetKeyDown("f"))
        {
            faceOn = !faceOn;
        }

        if( Input.GetKeyDown("e"))
        {
            edgeson = !edgeson;
        }
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
