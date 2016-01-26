using UnityEngine;
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
	private enum RectSide { NONE = 0, LEFT = 1, TOP = 2, RIGHT = 4, BOTTOM = 8 };

	private class MeshDescription
	{
		public Vector3[] vertices;
		public int[][] triangles;
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

	private bool IntersectLines(Vector2 fromA, Vector2 toA, Vector2 fromB, Vector2 toB, out Vector2 intersection)
	{
		Vector2 x1 = fromA;
		Vector2 v1 = toA - fromA;
		Vector2 x2 = fromB;
		Vector2 v2 = toB - fromB;

		// Check if parallel.
		if (Vector2.Dot(v1, v1) * Vector2.Dot(v2, v2) == Mathf.Pow(Vector2.Dot(v1, v2), 2))
		{
			intersection = Vector2.zero;
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
				intersection = x1 + (a * v1);
				return true;
			}
			else
			{
				intersection = Vector2.zero;
				return false;
			}
		}
	}

	private bool IntersectLineWithRectangle(Vector2 from, Vector2 to, Rect rectangle, int maxIntersections, out Vector2[] intersections, out RectSide sides)
	{
		bool intersected = false;
		sides = RectSide.NONE;
		Vector2 intersection;
		List<Vector2> intersectionsList = new List<Vector2>(maxIntersections);

		if (IntersectLines(from, to, new Vector2(rectangle.xMin, rectangle.yMin),
									 new Vector2(rectangle.xMin, rectangle.yMax), out intersection))
		{
			intersectionsList.Add(intersection);
			sides = RectSide.LEFT;
			intersected = true;
			if (intersectionsList.Count == maxIntersections)
			{
				intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(from, to, new Vector2(rectangle.xMin, rectangle.yMax),
									 new Vector2(rectangle.xMax, rectangle.yMax), out intersection))
		{
			intersectionsList.Add(intersection);
			sides = sides & RectSide.TOP;
			intersected = true;
			if (intersectionsList.Count == maxIntersections)
			{
				intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(from, to, new Vector2(rectangle.xMax, rectangle.yMax),
									 new Vector2(rectangle.xMax, rectangle.yMin), out intersection))
		{
			intersectionsList.Add(intersection);
			sides = sides & RectSide.RIGHT;
			intersected = true;
			if (intersectionsList.Count == maxIntersections)
			{
				intersections = intersectionsList.ToArray();
				return true;
			}
		}
		if (IntersectLines(from, to, new Vector2(rectangle.xMax, rectangle.yMin),
									 new Vector2(rectangle.xMin, rectangle.yMin), out intersection))
		{
			intersectionsList.Add(intersection);
			sides = sides & RectSide.BOTTOM;
			intersected = true;
		}
		intersections = intersectionsList.ToArray();
		return intersected;
	}

	private void FindClippingVoronoiEdges(Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges, List<Vector2> a_ClippingEdges)
	{
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
					if (m_MeshRect.Contains(voronoiPos) == false)
					{
						if (m_MeshRect.Contains(adjacentVoronoiPos))
						{
							Vector2[] intersections;
							RectSide intersectedSides;
							if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 1, out intersections,
								out intersectedSides))
							{
								a_ClippingEdges.Add(voronoiPos);
								a_ClippingEdges.Add(intersections[0]);
							}
						}
						else
						{
							Vector2[] intersections;
							RectSide intersectedSides;
							if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 2, out intersections,
								out intersectedSides))
							{
								int index = 0;
								if ((intersectedSides & RectSide.LEFT) != RectSide.NONE)
								{
									if (voronoiPos.x < m_MeshRect.xMin)
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[0]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & RectSide.TOP) != RectSide.NONE)
								{
									if (voronoiPos.y > m_MeshRect.yMax)
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[index]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & RectSide.RIGHT) != RectSide.NONE)
								{
									if (voronoiPos.x > m_MeshRect.xMax) 
									{
										a_ClippingEdges.Add(voronoiPos);
										a_ClippingEdges.Add(intersections[index]);
										continue;
									}
									index++;
								}
								if ((intersectedSides & RectSide.BOTTOM) != RectSide.NONE)
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
								a_ClippingEdges.Add(voronoiPos);
								a_ClippingEdges.Add(adjacentVoronoiPos);
							}
						}
					}
				}
			}
		}
	}

	private MeshDescription TriangulateVoronoi()
	{
		Dictionary<Vertex, HashSet<Vertex>> internalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		Dictionary<Vertex, HashSet<Vertex>> voronoiEdges = new Dictionary<Vertex, HashSet<Vertex>>();
		foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
		{
			ProcessHalfEdge(halfEdge, voronoiEdges, internalEdges);
		}
		FindClippingVoronoiEdges(voronoiEdges, m_ClippingEdges);
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