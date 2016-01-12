using UnityEngine;
using System.Collections.Generic;
using System;
using Voronoi;

public class GraphManager : MonoBehaviour
{
	public Material[] m_Materials;
	public GameObject m_OnClickObjectPrefab;
	private Delaunay m_Delaunay;
	private bool m_CircleOn = false;
	private bool m_EdgesOn = false;
	private bool m_VoronoiOn = true;
	private MeshFilter m_MeshFilter;
	private bool player1Turn = true;

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

    private void UpdateVoronoiMesh()
    {
		MeshDescription newDescription = TriangulateVoronoi();

		if (m_MeshFilter == null)
		{
			GameObject rendererObject = new GameObject("VoronoiRenderer");
			rendererObject.isStatic = true;
			rendererObject.transform.eulerAngles = new Vector3(270, 0, 0);
			MeshRenderer meshRenderer = rendererObject.AddComponent<MeshRenderer>();
			Material[] newMaterials = new Material[2];
			newMaterials[0] = m_Materials[0];
			newMaterials[1] = m_Materials[1];
			meshRenderer.materials = newMaterials;
			m_MeshFilter = rendererObject.AddComponent<MeshFilter>();
		}
		Mesh mesh = m_MeshFilter.mesh;
		if (mesh == null)
		{ mesh = new Mesh(); }
		mesh.subMeshCount = 2;
		mesh.MarkDynamic();
		mesh.vertices = newDescription.vertices;
		mesh.SetTriangles(newDescription.triangles[0], 0);
		mesh.SetTriangles(newDescription.triangles[1], 1);
		mesh.RecalculateBounds();

		Bounds bounds = mesh.bounds;
		//float width = (bounds.max - bounds.min).x;
		//float height = (bounds.max - bounds.min).z;
		Vector2[] newUVs = new Vector2[newDescription.vertices.Length];
		for (int i = 0; i < newDescription.vertices.Length; ++i)
		{
			Vector3 vertex = newDescription.vertices[i];
			newUVs[i] = new Vector2(vertex.x, vertex.z);
		}

		mesh.uv = newUVs;
		mesh.Optimize();
    }

    private void DrawCircles()
    {
        float radius = 0;
        GL.Begin(GL.LINES);

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

    private void DrawVoronoi()
    {
        GL.Begin(GL.LINES);
        foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
        {
            if (halfEdge.Twin == null)
			{ continue; }

            Triangle t1 = halfEdge.Triangle;
            Triangle t2 = halfEdge.Twin.Triangle;

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

	private MeshDescription TriangulateVoronoi()
	{
		Dictionary<Vertex, HashSet<Vertex>> internalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		Dictionary<Vertex, HashSet<Vertex>> voronoiEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
		{
			ProcessHalfEdge(halfEdge, voronoiEdges, internalEdges);
		}
		List<Vector3> vertices = new List<Vector3>();
		List<int>[] triangleLists = new List<int>[2];
		triangleLists[0] = new List<int>();
		triangleLists[1] = new List<int>();
		foreach (Vertex inputNode in internalEdges.Keys)
		{
			bool unowned = inputNode.Ownership == Vertex.EOwnership.UNOWNED;
			int playerIndex = inputNode.Ownership == Vertex.EOwnership.PLAYER1 ? 0 : -1;
			playerIndex = inputNode.Ownership == Vertex.EOwnership.PLAYER2 ? 1 : playerIndex;
			HashSet<Vertex> voronoiNodes = internalEdges[inputNode];
			foreach (Vertex voronoiNode in voronoiNodes)
			{
				HashSet<Vertex> adjacentVoronoiNodes = voronoiEdges[voronoiNode];
				HashSet<Vertex> intersection = new HashSet<Vertex>(adjacentVoronoiNodes, adjacentVoronoiNodes.Comparer);
				intersection.IntersectWith (voronoiNodes);
				foreach (Vertex adjacent in intersection)
				{
					int curCount = vertices.Count;
					vertices.Add(new Vector3(inputNode.X, 0, inputNode.Y));
					vertices.Add(new Vector3(voronoiNode.X, 0, voronoiNode.Y));
					vertices.Add(new Vector3(adjacent.X, 0, adjacent.Y));
					if (unowned)
					{
						triangleLists [0].Add (curCount);
						triangleLists [0].Add (curCount + 1);
						triangleLists [0].Add (curCount + 2);
						triangleLists [1].Add (curCount);
						triangleLists [1].Add (curCount + 1);
						triangleLists [1].Add (curCount + 2);
					}
					else
					{
						triangleLists [playerIndex].Add (curCount);
						triangleLists [playerIndex].Add (curCount + 1);
						triangleLists [playerIndex].Add (curCount + 2);
					}
				}
			}
		}
		MeshDescription description = new MeshDescription();
		description.triangles = new int[2][];
		description.triangles[0] = triangleLists[0].ToArray();
		description.triangles[1] = triangleLists[1].ToArray();
		description.vertices = vertices.ToArray();
		return description;

		/**GL.Begin(GL.LINES);
		foreach (Vertex key in internalEdges.Keys)
		{
			HashSet<Vertex> vertices = internalEdges[key];
			foreach (Vertex item in vertices)
			{
				GL.Vertex3(key.X, 0, key.Y);
				GL.Vertex3(item.X, 0, item.Y);
			}
		}
		foreach (Vertex key in voronoiEdges.Keys)
		{
			HashSet<Vertex> vertices = voronoiEdges[key];
			foreach (Vertex item in vertices)
			{
				GL.Vertex3(key.X, 0, key.Y);
				GL.Vertex3(item.X, 0, item.Y);
			}
		}
		GL.End();**/
	}

	private class MeshDescription
	{
		public Vector3[] vertices;
		public int[][] triangles;
	}

	private void ProcessHalfEdge(HalfEdge h1, Dictionary<Vertex, HashSet<Vertex>> voronoiEdges, Dictionary<Vertex, HashSet<Vertex>> internalEdges)
	{
		if (h1.Twin == null)
		{ return; }

		Triangle t1 = h1.Triangle;
		Triangle t2 = h1.Twin.Triangle;

		if (t1 != null && t2 != null)
		{
			Vertex voronoiVertex = t1.Circumcenter;
			Vertex voronoiVertex2 = t2.Circumcenter;
			HashSet<Vertex> existingVoronoiEdges;

			if (voronoiEdges.TryGetValue(voronoiVertex, out existingVoronoiEdges))
			{ existingVoronoiEdges.Add(voronoiVertex2); }
			else
			{ voronoiEdges.Add(voronoiVertex, new HashSet<Vertex>{voronoiVertex2}); }

			if (voronoiEdges.TryGetValue (voronoiVertex2, out existingVoronoiEdges))
			{ existingVoronoiEdges.Add(voronoiVertex); }
			else
			{ voronoiEdges.Add(voronoiVertex2, new HashSet<Vertex>{voronoiVertex}); }

			foreach (Vertex inputVertex in t1.Vertices)
			{
				HashSet<Vertex> existingValue;
				if (internalEdges.TryGetValue(inputVertex, out existingValue))
				{ existingValue.Add(voronoiVertex); }
				else
				{ internalEdges.Add(inputVertex, new HashSet<Vertex>{voronoiVertex}); }
			}
			// Yes, yes, code duplication is bad.
			foreach (Vertex inputVertex in t2.Vertices)
			{
				HashSet<Vertex> existingValue;
				if (internalEdges.TryGetValue(inputVertex, out existingValue))
				{ existingValue.Add(voronoiVertex2); }
				else
				{ internalEdges.Add(inputVertex, new HashSet<Vertex>{voronoiVertex2}); }
			}
		}
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

        if (m_CircleOn)
        { DrawCircles(); }

		if (m_VoronoiOn)
		{ DrawVoronoi(); }

        GL.PopMatrix();
    }

    private void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            m_CircleOn = !m_CircleOn;
        }

        if (Input.GetKeyDown("e"))
        {
            m_EdgesOn = !m_EdgesOn;
        }

		if (Input.GetKeyDown("v"))
		{
			m_VoronoiOn = !m_VoronoiOn;
		}

		if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pos.z = 0;
			Vertex me = new Vertex(pos.x, pos.y, player1Turn ? Vertex.EOwnership.PLAYER1 : Vertex.EOwnership.PLAYER2);
			player1Turn = !player1Turn;
			m_Delaunay.AddVertex(me);

			GameObject onClickObject = GameObject.Instantiate(m_OnClickObjectPrefab, pos, Quaternion.identity) as GameObject;
			if (onClickObject == null)
			{ Debug.LogError("Couldn't instantiate m_OnClickObjectPrefab!"); }

			UpdateVoronoiMesh();
		}
    }
}