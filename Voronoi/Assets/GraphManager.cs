﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Voronoi;

public class GraphManager : MonoBehaviour
{
	private Delaunay m_Delaunay;
	private bool m_CircleOn = false;
	private bool m_FaceOn = false;
	private bool m_EdgesOn = false;
	private bool m_TriangulationOn = false;
	private bool m_VoronoiOn = true;
	private GameObject m_VoronoiRendererObject;

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
		if (m_VoronoiRendererObject == null)
		{
			m_VoronoiRendererObject = new GameObject("VoronoiRenderer");
			m_VoronoiRendererObject.AddComponent<MeshRenderer>();
			MeshFilter filter = m_VoronoiRendererObject.AddComponent<MeshFilter>();
			Mesh mesh = filter.mesh;
			mesh.Clear();
			mesh.MarkDynamic();
			List<int> triangles = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			//List<Vector2> UVs = new List<Vector2>();
			int counter = 0;
			foreach (Triangle triangle in m_Delaunay.Triangles)
			{
				vertices.Add (new Vector3 (triangle.HalfEdge.Origin.X, triangle.HalfEdge.Origin.Y, -1));
				vertices.Add (new Vector3 (triangle.HalfEdge.Next.Origin.X, triangle.HalfEdge.Next.Origin.Y, -1));
				vertices.Add (new Vector3 (triangle.HalfEdge.Prev.Origin.X, triangle.HalfEdge.Prev.Origin.Y, -1));

				//UVs.Add (new Vector2 (0, 1));
				//UVs.Add (new Vector2 (1, 1));
				//UVs.Add (new Vector2 (0, 0));

				triangles.Add(counter + 2);
				triangles.Add(counter + 1);
				triangles.Add(counter);

				counter += 3;
				triangle.Drawn = true;
			}
			mesh.vertices = vertices.ToArray();
			//mesh.uv = UVs.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.Optimize();
		}
		/**
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
        **/
    }

    private void DrawCircles()
    {
        float radius = 0;
        GL.Begin(GL.LINES);

        //System.Random rand = new System.Random();

        //GL.Color(new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));

		foreach (Triangle triangle in m_Delaunay.Triangles)
        {
            GL.Color(triangle.Color);
            radius = Convert.ToSingle(Math.Sqrt(triangle.CircumcenterRangeSquared));
            float heading = 0;
            float extra = (360 / 100);
            for (int a = 0; a < (360 + extra); a += 360 / 100)
            {
                //the circle.
                GL.Vertex3((Mathf.Cos(heading) * radius) + triangle.Circumcenter.X, 0, (Mathf.Sin(heading) * radius) + triangle.Circumcenter.Y);
                heading = a * Mathf.PI / 180;
                GL.Vertex3((Mathf.Cos(heading) * radius) + triangle.Circumcenter.X, 0, (Mathf.Sin(heading) * radius) + triangle.Circumcenter.Y);

                //midpoint of the circle.
                GL.Vertex3((Mathf.Cos(heading) * 0.1f) + triangle.Circumcenter.X, 0, (Mathf.Sin(heading) * 0.1f) + triangle.Circumcenter.Y);
                GL.Vertex3((Mathf.Cos(heading) * 0.2f) + triangle.Circumcenter.X, 0, (Mathf.Sin(heading) * 0.2f) + triangle.Circumcenter.Y);
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

            Triangle t1 = halfEdge.Triangle as Triangle;
            Triangle t2 = halfEdge.Twin.Triangle as Triangle;

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

	private void TriangulateVoronoi()
	{
		Dictionary<Vertex, List<Vertex>> edges = new Dictionary<Vertex, List<Vertex>> ();
		foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
		{
			if (halfEdge.Twin == null)
			{ continue; }

			Triangle t1 = halfEdge.Triangle as Triangle;
			Triangle t2 = halfEdge.Twin.Triangle as Triangle;

			if (t1 != null && t2 != null)
			{
				Vertex voronoiVertex = t1.Circumcenter;
				foreach (Vertex inputVertex in t1.Vertices)
				{
					List<Vertex> existingValue;
					if (edges.TryGetValue(inputVertex, out existingValue))
					{
						existingValue.Add(voronoiVertex);
					}
					else
					{
						edges.Add(inputVertex, new List<Vertex>{voronoiVertex});
					}
				}
				// Yes, yes, code duplication is bad.
				voronoiVertex = t2.Circumcenter;
				foreach (Vertex inputVertex in t2.Vertices)
				{
					List<Vertex> existingValue;
					if (edges.TryGetValue(inputVertex, out existingValue))
					{
						existingValue.Add(voronoiVertex);
					}
					else
					{
						edges.Add(inputVertex, new List<Vertex>{voronoiVertex});
					}
				}
			}
		}
		GL.Begin(GL.LINES);
		foreach (Vertex key in edges.Keys)
		{
			List<Vertex> vertices = edges[key];
			foreach (Vertex item in vertices)
			{
				GL.Vertex3(key.X, 0, key.Y);
				GL.Vertex3(item.X, 0, item.Y);
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

		if (m_VoronoiOn)
		{ DrawVoronoi(); }

		if (m_TriangulationOn)
		{ TriangulateVoronoi(); }

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

		if (Input.GetKeyDown("t"))
		{
			m_TriangulationOn = !m_TriangulationOn;
		}

		if (Input.GetKeyDown("v"))
		{
			m_VoronoiOn = !m_VoronoiOn;
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