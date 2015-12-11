using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Voronoi;

public class GraphManager : MonoBehaviour
{
    private Delaunay sander;
    private bool circleOn = false;
    private bool faceOn = false;
    private bool edgeson = false;

    private void Start()
    {
        sander = new Delaunay();
        sander.Create();

    }

    private void drawEdges()
    {
        GL.Begin(GL.LINES);
        // Vertex colors change from red to green

        foreach (HalfEdge halfEdge in sander.HalfEdges)
        {
            GL.Vertex3(halfEdge.Origin.X, 0, halfEdge.Origin.Y);
            GL.Vertex3(halfEdge.Next.Origin.X, 0, halfEdge.Next.Origin.Y);
        }
        GL.End();
    }


    public Vector2[] newUV;

    private void fillFaces()
    {
        GL.Begin(GL.TRIANGLES);

        foreach (Triangle face in sander.Faces)
        {

            if (!face.m_drawn)
            {

                Material noPlayer = Resources.Load("AreaNoPlayer", typeof(Material)) as Material;
                Material player1 = Resources.Load("AreaPlayer1", typeof(Material)) as Material;
                Material player2 = Resources.Load("AreaPlayer2", typeof(Material)) as Material;

                var go = new GameObject();
                var fil = go.AddComponent<MeshFilter>();
                var rend = go.AddComponent<MeshRenderer>();
                Mesh mesh = go.GetComponent<MeshFilter>().mesh;
                mesh.Clear();
                mesh.vertices = new Vector3[] {
                    new Vector3(face.HalfEdge.Origin.X, face.HalfEdge.Origin.Y, -1),
                    new Vector3(face.HalfEdge.Next.Origin.X, face.HalfEdge.Next.Origin.Y, -1),
                    new Vector3(face.HalfEdge.Prev.Origin.X, face.HalfEdge.Prev.Origin.Y, -1)
                };
                mesh.uv = new Vector2[]
                {
                    new Vector2( 0, 1 ),
                    new Vector2(1, 1),
                    new Vector2(0, 0)
                };

                mesh.triangles = new int[] { 2, 1, 0 };
                face.setDrawn(true);
                
                rend.material = player2;

            }

            //GL.Color(face.Color);
            //GL.Vertex3(face.HalfEdge.Origin.X, 0, face.HalfEdge.Origin.Y);
            //GL.Vertex3(face.HalfEdge.Next.Origin.X, 0, face.HalfEdge.Next.Origin.Y);
            //GL.Vertex3(face.HalfEdge.Prev.Origin.X, 0, face.HalfEdge.Prev.Origin.Y);
            
        }


        GL.End();

    }

    private void drawCircles()
    {
        float radius = 0;
        GL.Begin(GL.LINES);

        //System.Random rand = new System.Random();

        //GL.Color(new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));

        foreach (Triangle face in sander.Faces)
        {
            GL.Color(face.Color);
            radius = Convert.ToSingle(Math.Sqrt(face.CircumcenterRangeSquared));
            float heading = 0;
            float extra = (360 / 100);
            for (int a = 0; a < (360 + extra); a += 360 / 100)
            {
                //the circle.
                GL.Vertex3((Mathf.Cos(heading) * radius) + face.Circumcenter.X, 0, (Mathf.Sin(heading) * radius) + face.Circumcenter.Y);
                heading = a * Mathf.PI / 180;
                GL.Vertex3((Mathf.Cos(heading) * radius) + face.Circumcenter.X, 0, (Mathf.Sin(heading) * radius) + face.Circumcenter.Y);

                //midpoint of the circle.
                GL.Vertex3((Mathf.Cos(heading) * 0.1f) + face.Circumcenter.X, 0, (Mathf.Sin(heading) * 0.1f) + face.Circumcenter.Y);
                GL.Vertex3((Mathf.Cos(heading) * 0.2f) + face.Circumcenter.X, 0, (Mathf.Sin(heading) * 0.2f) + face.Circumcenter.Y);
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

            Triangle f1 = halfEdge.Face as Triangle;
            Triangle f2 = halfEdge.Twin.Face as Triangle;

            if (f1 is Triangle && f2 is Triangle)
            {
                Triangle t1 = f1 as Triangle;
                Triangle t2 = f2 as Triangle;

                Vertex v1 = t1.Circumcenter;
                Vertex v2 = t2.Circumcenter;

                GL.Vertex3(v1.X, 0, v1.Y);
                GL.Vertex3(v2.X, 0, v2.Y);
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

        if (Input.GetKeyDown("e"))
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