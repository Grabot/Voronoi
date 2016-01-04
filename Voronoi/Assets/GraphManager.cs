using UnityEngine;
using System.Collections.Generic;
using System;
using Voronoi;

public class GraphManager : MonoBehaviour
{
	private Delaunay m_Delaunay;
	private bool m_CircleOn = false;
	private bool m_FaceOn = false;
	private bool m_EdgesOn = false;
	public Vector2[] newUV;

    private void Start()
    {
        m_Delaunay = new Delaunay();
        m_Delaunay.Create();
    }

    private void DrawEdges()
    {
        GL.Begin(GL.LINES);
        // Vertex colors change from red to green

        foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
        {
            GL.Vertex3(halfEdge.Origin.X, 0, halfEdge.Origin.Y);
            GL.Vertex3(halfEdge.Next.Origin.X, 0, halfEdge.Next.Origin.Y);
        }
        GL.End();
    }

    private void FillFaces()
    {
        foreach (Triangle face in m_Delaunay.Faces)
        {
			if (!face.Drawn)
			{
				Material player2 = Resources.Load ("AreaPlayer2", typeof(Material)) as Material;
				if (player2 != null)
				{
					GameObject go = new GameObject ();
					MeshRenderer rend = go.AddComponent<MeshRenderer>();
					MeshFilter filt = go.AddComponent<MeshFilter>();
					Mesh mesh = filt.mesh;
					mesh.vertices = new Vector3[] {
						new Vector3 (face.HalfEdge.Origin.X, face.HalfEdge.Origin.Y, -1),
						new Vector3 (face.HalfEdge.Next.Origin.X, face.HalfEdge.Next.Origin.Y, -1),
						new Vector3 (face.HalfEdge.Prev.Origin.X, face.HalfEdge.Prev.Origin.Y, -1)
					};
					mesh.uv = new Vector2[] {
						new Vector2 (0, 1),
						new Vector2 (1, 1),
						new Vector2 (0, 0)
					};
					mesh.triangles = new int[] { 2, 1, 0 };
					face.Drawn = true;

					rend.material = player2;
				}
				else
				{
					Debug.LogError("The AreaPlayer2 material cannot be loaded!");
				}
			}
            //GL.Color(face.Color);
            //GL.Vertex3(face.HalfEdge.Origin.X, 0, face.HalfEdge.Origin.Y);
            //GL.Vertex3(face.HalfEdge.Next.Origin.X, 0, face.HalfEdge.Next.Origin.Y);
            //GL.Vertex3(face.HalfEdge.Prev.Origin.X, 0, face.HalfEdge.Prev.Origin.Y); 
        }
    }

    private void DrawCircles()
    {
        float radius = 0;
        GL.Begin(GL.LINES);

        //System.Random rand = new System.Random();

        //GL.Color(new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));

        foreach (Triangle face in m_Delaunay.Faces)
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
    private void DrawVoronoi()
    {
        GL.Begin(GL.LINES);
        foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
        {
            if (halfEdge.Twin == null)
			{ continue; }

            Triangle t1 = halfEdge.Face as Triangle;
            Triangle t2 = halfEdge.Twin.Face as Triangle;

            if (t1 != null && t2 != null)
            {
                Vertex v1 = t1.Circumcenter;
                Vertex v2 = t2.Circumcenter;

                GL.Vertex3(v1.X, 0, v1.Y);
                GL.Vertex3(v2.X, 0, v2.Y);
            }
        }
        GL.End();
    }

    private void OnRenderObject()
    {
        m_Delaunay.CreateLineMaterial();
        // Apply the line material
        m_Delaunay.m_LineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        if (m_EdgesOn)
        { DrawEdges(); }

        if (m_FaceOn)
        { FillFaces(); }

        if (m_CircleOn)
        { DrawCircles(); }

        DrawVoronoi();

        GL.PopMatrix();
    }

    private void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            m_CircleOn = !m_CircleOn;
        }

        if (Input.GetKeyDown("f"))
        {
            m_FaceOn = !m_FaceOn;
        }

        if (Input.GetKeyDown("e"))
        {
            m_EdgesOn = !m_EdgesOn;
        }
    }

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            Vector3 newPos = hits[0].point;
            Vertex me = new Vertex(newPos.x, newPos.y);
            m_Delaunay.AddVertex(me);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}