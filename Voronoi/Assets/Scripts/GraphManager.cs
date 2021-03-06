﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Voronoi;

public class GraphManager : MonoBehaviour
{
	public GameObject m_OnClickObjectPrefab;
	private Delaunay m_Delaunay;
	private bool m_CircleOn = false;
	private bool m_EdgesOn = false;
	private bool m_VoronoiOn = true;
	private MeshFilter m_MeshFilter;
	private bool player1Turn = true;
	private Transform m_MyTransform;
	public GUIManager m_GUIManager;
	private FishManager m_FishManager;
	private Rect m_MeshRect;
	private List<Vector2> m_ClippingEdges = new List<Vector2>();

	[Flags]
	private enum ERectangleSide { NONE = 0, LEFT = 1, TOP = 2, RIGHT = 4, BOTTOM = 8 };

	private class MeshDescription
	{
		public Vector3[] vertices;
		public int[][] triangles;
	}

	private struct InvalidEdge
	{
		public Vertex m_InvalidVertex;
		public Vector2 m_IntersectingPoint;

		public InvalidEdge(Vertex a_InvalidVertex, Vector2 a_IntersectingPoint)
		{
			m_InvalidVertex = a_InvalidVertex;
			m_IntersectingPoint = a_IntersectingPoint;
		}
	}

	void Awake()
	{
		m_MyTransform = this.gameObject.transform;
		m_FishManager = new FishManager();
		GameObject rendererObject = GameObject.Find("VoronoiMesh");
		m_MeshFilter = rendererObject.GetComponent<MeshFilter>();
		float z = (m_MeshFilter.transform.position - Camera.main.transform.position).magnitude;
		Vector3 bottomLeft = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, z));
		Vector3 topRight = Camera.main.ViewportToWorldPoint (new Vector3 (1, 1, z));
		m_MeshRect = new Rect (bottomLeft.x, bottomLeft.z, topRight.x - bottomLeft.x, topRight.z - bottomLeft.z);
	}

    private void Start()
    {
        m_Delaunay = new Delaunay();
        m_Delaunay.Create();
    }

    private void DrawEdges()
    {
        GL.Begin(GL.LINES);

        foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
        {
            if (halfEdge.CanEat())
            {
                GL.Color(Color.red);
            }
            else
            {
                GL.Color(Color.green);
            }

            GL.Vertex3(halfEdge.Origin.X, 0, halfEdge.Origin.Y);
            GL.Vertex3(halfEdge.Next.Origin.X, 0, halfEdge.Next.Origin.Y);
        }
        GL.End();
    }

    private void UpdateVoronoiMesh()
    {
		MeshDescription newDescription = TriangulateVoronoi();
		Mesh mesh = m_MeshFilter.mesh;
		if (mesh == null)
		{
			mesh = new Mesh();
			m_MeshFilter.mesh = mesh;
		}
		mesh.subMeshCount = 2;
		mesh.MarkDynamic();
		mesh.vertices = newDescription.vertices;
		mesh.SetTriangles(newDescription.triangles[0], 0);
		mesh.SetTriangles(newDescription.triangles[1], 1);
		mesh.RecalculateBounds();

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

	private static bool IntersectLines(Vector2 a_FromA, Vector2 a_ToA, Vector2 a_FromB, Vector2 a_ToB, out Vector2 o_Intersection)
	{
		Vector2 x1 = a_FromA;
		Vector2 v1 = a_ToA - a_FromA;
		Vector2 x2 = a_FromB;
		Vector2 v2 = a_ToB - a_FromB;

		// Check if parallel.
		if (Vector2.Dot(v1, v1) * Vector2.Dot(v2, v2) == Mathf.Pow(Vector2.Dot(v1, v2), 2))
		{
			o_Intersection = Vector2.zero;
			return false;
		}
		else
		{
			float a = ((Vector2.Dot(v2, v2) * Vector2.Dot(v1, x2 - x1)) - (Vector2.Dot(v1, v2) * Vector2.Dot(v2, x2 - x1)))
				/ ((Vector2.Dot(v1, v1) * Vector2.Dot(v2, v2)) - Mathf.Pow(Vector2.Dot(v1, v2), 2));
			float b = ((Vector2.Dot(v1, v2) * Vector2.Dot(v1, x2 - x1)) - (Vector2.Dot(v1, v1) * Vector2.Dot(v2, x2 - x1)))
				/ ((Vector2.Dot(v1, v1) * Vector2.Dot(v2, v2)) - Mathf.Pow(Vector2.Dot(v1, v2), 2));
			// Check if intersection point is on the line segments.
			if (0 <= a && a <= 1 && 0 <= b && b <= 1)
			{
				o_Intersection = x1 + (a * v1);
				return true;
			}
			else
			{
				o_Intersection = Vector2.zero;
				return false;
			}
		}
	}

	private static bool IntersectLineWithRectangle(Vector2 a_From, Vector2 a_To, Rect a_Rectangle, int a_MaxIntersections, out Vector2[] o_Intersections,
																														   out ERectangleSide o_Sides)
	{
		bool intersected = false;
		o_Sides = ERectangleSide.NONE;
		Vector2 intersection;
		List<Vector2> intersectionsList = new List<Vector2>(a_MaxIntersections);

		if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMin, a_Rectangle.yMin),
									 new Vector2(a_Rectangle.xMin, a_Rectangle.yMax), out intersection))
		{
			intersectionsList.Add(intersection);
			o_Sides = ERectangleSide.LEFT;
			intersected = true;
			if (intersectionsList.Count == a_MaxIntersections)
			{
				o_Intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMin, a_Rectangle.yMax),
									 new Vector2(a_Rectangle.xMax, a_Rectangle.yMax), out intersection))
		{
			intersectionsList.Add(intersection);
			o_Sides = o_Sides & ERectangleSide.TOP;
			intersected = true;
			if (intersectionsList.Count == a_MaxIntersections)
			{
				o_Intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMax, a_Rectangle.yMax),
									 new Vector2(a_Rectangle.xMax, a_Rectangle.yMin), out intersection))
		{
			intersectionsList.Add(intersection);
			o_Sides = o_Sides & ERectangleSide.RIGHT;
			intersected = true;
			if (intersectionsList.Count == a_MaxIntersections)
			{
				o_Intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMax, a_Rectangle.yMin),
									 new Vector2(a_Rectangle.xMin, a_Rectangle.yMin), out intersection))
		{
			intersectionsList.Add(intersection);
			o_Sides = o_Sides & ERectangleSide.BOTTOM;
			intersected = true;
		}
		o_Intersections = intersectionsList.ToArray();
		return intersected;
	}

	private void ReplaceVoronoiVertex(Vertex a_InvalidVertex, Vertex a_ReplacingVertex, Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
																						Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternal,
																						Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges)
	{
		HashSet<Vertex> adjacentVoronoiVertices;
		if (a_VoronoiEdges.TryGetValue(a_InvalidVertex, out adjacentVoronoiVertices))
		{
			if (a_ReplacingVertex != null)
			{ a_VoronoiEdges[a_ReplacingVertex] = adjacentVoronoiVertices; }
			a_VoronoiEdges.Remove(a_InvalidVertex);
			foreach (Vertex adjacentVertex in adjacentVoronoiVertices)
			{
				HashSet<Vertex> adjacentOfAdjacent;
				if (a_VoronoiEdges.TryGetValue(adjacentVertex, out adjacentOfAdjacent))
				{
					adjacentOfAdjacent.Remove(a_InvalidVertex);
					if (a_ReplacingVertex != null)
					{ adjacentOfAdjacent.Add(a_ReplacingVertex); }
				}
			}
		}
		HashSet<Vertex> inputVertices;
		if (a_VoronoiToInternal.TryGetValue(a_InvalidVertex, out inputVertices))
		{
			if (a_ReplacingVertex != null)
			{ a_VoronoiToInternal[a_ReplacingVertex] = inputVertices; }
			a_VoronoiToInternal.Remove(a_InvalidVertex);
			foreach (Vertex inputVertex in inputVertices)
			{
				HashSet<Vertex> voronoiVertices;
				if (a_InternalEdges.TryGetValue(inputVertex, out voronoiVertices))
				{
					voronoiVertices.Remove(a_InvalidVertex);
					if (a_ReplacingVertex != null)
					{ voronoiVertices.Add (a_ReplacingVertex); }
				}
			}
		}
	}

	private void FixInvalidVoronoiEdges(List<InvalidEdge> a_InvalidEdges, List<Vertex> a_VerticesToRemove,
																		  Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
																		  Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges,
																		  Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternal)
	{
		foreach (InvalidEdge invalidEdge in a_InvalidEdges)
		{
			Vertex replacingVertex = new Vertex(invalidEdge.m_IntersectingPoint.x, invalidEdge.m_IntersectingPoint.y, invalidEdge.m_InvalidVertex.Ownership);
			ReplaceVoronoiVertex(invalidEdge.m_InvalidVertex, replacingVertex, a_VoronoiEdges, a_VoronoiToInternal, a_InternalEdges);
		}
		foreach (Vertex invalidVertex in a_VerticesToRemove)
		{
			ReplaceVoronoiVertex(invalidVertex, null, a_VoronoiEdges, a_VoronoiToInternal, a_InternalEdges);
		}
	}

	private List<InvalidEdge> FindClippingVoronoiEdges(Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges, List<Vertex> a_VerticesToRemove, List<Vector2> a_ClippingEdges)
	{
		List<InvalidEdge> invalidEdges = new List<InvalidEdge>();
		a_ClippingEdges.Clear();
		foreach (Vertex voronoiVertex in a_VoronoiEdges.Keys)
		{
			Vector2 voronoiPos = new Vector2(voronoiVertex.X, voronoiVertex.Y);
			if (m_MeshRect.Contains(voronoiPos) == false)
			{
				HashSet<Vertex> adjacentVoronoiVertices = a_VoronoiEdges[voronoiVertex];
				foreach (Vertex adjacentVertex in adjacentVoronoiVertices)
				{
					Vector2 adjacentVoronoiPos = new Vector2(adjacentVertex.X, adjacentVertex.Y);
					// For an edge to be invalid, it either has 1 vertex outside the rectangle (clipping it)
					// or both vertices are completely outside of the rectangle and not clipping it
					// or both vertices are completely outside of the rectangle and the edge intersects the rectangle twice.
					if (m_MeshRect.Contains(voronoiPos) == false)
					{
						// If the first vertex is outside and the second vertex is inside, we intersect the rectangle in 1 location.
						if (m_MeshRect.Contains(adjacentVoronoiPos))
						{
							Vector2[] intersections;
							ERectangleSide intersectedSides;
							if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 1, out intersections,
								out intersectedSides))
							{
								a_ClippingEdges.Add(voronoiPos);
								a_ClippingEdges.Add(intersections[0]);
								invalidEdges.Add(new InvalidEdge(voronoiVertex, intersections[0]));
							}
						}
						else
						{
							// If the first vertex it outside and the second vertex too, it is possible we still intersect the
							// rectangle in 2 places.
							Vector2[] intersections;
							ERectangleSide intersectedSides;
							if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 2, out intersections,
								out intersectedSides))
							{
								// The edge intersects the rectangle in 2 places, find the intersection on the rectangle that is
								// on "this side" of the rectangle, seen from the first vertex of the edge that we are processing.
								// The line segment part of the edge on the other side of the rectangle will be found later in the iteration.
								int index = 0;
								if ((intersectedSides & ERectangleSide.LEFT) != ERectangleSide.NONE)
								{
									if (voronoiPos.x < m_MeshRect.xMin)
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[0]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & ERectangleSide.TOP) != ERectangleSide.NONE)
								{
									if (voronoiPos.y > m_MeshRect.yMax)
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[index]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & ERectangleSide.RIGHT) != ERectangleSide.NONE)
								{
									if (voronoiPos.x > m_MeshRect.xMax) 
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[index]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & ERectangleSide.BOTTOM) != ERectangleSide.NONE)
								{
									if (voronoiPos.y < m_MeshRect.yMin)
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[index]);
									}
								}
							}
							else
							{
								// Both vertices are outside of the rectangle and the edge does not intersect with the rectangle.
								a_ClippingEdges.Add(voronoiPos);
								a_ClippingEdges.Add(adjacentVoronoiPos);
								//a_VerticesToRemove.Add(voronoiVertex);
								//a_VerticesToRemove.Add(adjacentVertex);
							}
						}
					}
				}
			}
		}
		return invalidEdges;
	}

	private MeshDescription TriangulateVoronoi()
	{
		Dictionary<Vertex, HashSet<Vertex>> internalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		Dictionary<Vertex, HashSet<Vertex>> voronoiEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		Dictionary<Vertex, HashSet<Vertex>> voronoiToInternalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
		{
			ProcessHalfEdge(halfEdge, voronoiEdges, internalEdges, voronoiToInternalEdges);
		}
		List<Vertex> verticesToRemove = new List<Vertex>();
		List<InvalidEdge> invalidEdges = FindClippingVoronoiEdges(voronoiEdges, verticesToRemove, m_ClippingEdges);
		FixInvalidVoronoiEdges(invalidEdges, verticesToRemove, voronoiEdges, internalEdges, voronoiToInternalEdges);
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
				intersection.IntersectWith(voronoiNodes);
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

	private static void ProcessHalfEdge(HalfEdge a_H1, Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
										Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges,
										Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternalEdges)
	{
		if (a_H1.Twin == null)
		{ return; }

		Triangle t1 = a_H1.Triangle;
		Triangle t2 = a_H1.Twin.Triangle;

		if (t1 != null && t2 != null)
		{
			Vertex voronoiVertex = t1.Circumcenter;
			Vertex voronoiVertex2 = t2.Circumcenter;
			HashSet<Vertex> existingVoronoiEdges;

			if (a_VoronoiEdges.TryGetValue(voronoiVertex, out existingVoronoiEdges))
			{ existingVoronoiEdges.Add(voronoiVertex2); }
			else
			{ a_VoronoiEdges.Add(voronoiVertex, new HashSet<Vertex>{voronoiVertex2}); }

			if (a_VoronoiEdges.TryGetValue (voronoiVertex2, out existingVoronoiEdges))
			{ existingVoronoiEdges.Add(voronoiVertex); }
			else
			{ a_VoronoiEdges.Add(voronoiVertex2, new HashSet<Vertex>{voronoiVertex}); }

			foreach (Vertex inputVertex in t1.Vertices)
			{
				HashSet<Vertex> existingValue;
				if (a_InternalEdges.TryGetValue(inputVertex, out existingValue))
				{ existingValue.Add(voronoiVertex); }
				else
				{ a_InternalEdges.Add(inputVertex, new HashSet<Vertex>{voronoiVertex}); }
			}
			HashSet<Vertex> inputVertices = new HashSet<Vertex>(t1.Vertices);
			HashSet<Vertex> existingInputVertices;
			if (a_VoronoiToInternalEdges.TryGetValue(voronoiVertex, out existingInputVertices))
			{ existingInputVertices.UnionWith(inputVertices); }
			else
			{ a_VoronoiToInternalEdges.Add(voronoiVertex, inputVertices); }
			// Yes, yes, code duplication is bad.
			foreach (Vertex inputVertex in t2.Vertices)
			{
				HashSet<Vertex> existingValue;
				if (a_InternalEdges.TryGetValue(inputVertex, out existingValue))
				{ existingValue.Add(voronoiVertex2); }
				else
				{ a_InternalEdges.Add(inputVertex, new HashSet<Vertex>{voronoiVertex2}); }
			}
			inputVertices = new HashSet<Vertex>(t2.Vertices);
			existingInputVertices = null;
			if (a_VoronoiToInternalEdges.TryGetValue(voronoiVertex2, out existingInputVertices))
			{ existingInputVertices.UnionWith(inputVertices); }
			else
			{ a_VoronoiToInternalEdges.Add(voronoiVertex2, inputVertices); }
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

		DrawMeshRect();
		DrawInvalidVoronoiEdges();

        GL.PopMatrix();
    }

	private void DrawInvalidVoronoiEdges()
	{
		GL.Begin(GL.LINES);
		GL.Color(Color.red);
		for (int i = 0; i < m_ClippingEdges.Count; i += 2)
		{
			GL.Vertex3(m_ClippingEdges[i].x, 0, m_ClippingEdges[i].y);
			GL.Vertex3(m_ClippingEdges[i + 1].x, 0, m_ClippingEdges[i + 1].y);
		}
		GL.End();
	}

	private void DrawMeshRect()
	{
		GL.Begin(GL.LINES);

		GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMin);
		GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMax);

		GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMax);
		GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMax);

		GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMax);
		GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMin);

		GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMin);
		GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMin);

		GL.End();
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
			pos.y = 0;
			Vertex me = new Vertex(pos.x, pos.z, player1Turn ? Vertex.EOwnership.PLAYER1 : Vertex.EOwnership.PLAYER2);
			m_Delaunay.AddVertex(me);

			GameObject onClickObject = GameObject.Instantiate(m_OnClickObjectPrefab, pos, Quaternion.identity) as GameObject;
			if (onClickObject == null)
			{ Debug.LogError ("Couldn't instantiate m_OnClickObjectPrefab!"); }
			else
			{
				onClickObject.name = "onClickObject_" + me.Ownership.ToString();
				onClickObject.transform.parent = m_MyTransform;
				m_FishManager.AddFish (onClickObject.transform, player1Turn ? 1 : 2);
			}

			UpdateVoronoiMesh();

			player1Turn = !player1Turn;
			if (player1Turn)
			{ m_GUIManager.OnBlueTurnStart(); }
			else
			{ m_GUIManager.OnRedTurnStart(); }
		}
    }
}