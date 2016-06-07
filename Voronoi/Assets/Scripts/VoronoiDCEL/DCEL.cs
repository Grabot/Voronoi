using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoronoiDCEL
{
	public class DCEL
	{
		private List<Vertex> m_Vertices;
		private List<Edge> m_Edges;
		private List<HalfEdge> m_HalfEdges;
		private List<Face> m_Faces;

		public DCEL()
		{
			m_Vertices = new List<Vertex>();
			m_Edges = new List<Edge>();
			m_HalfEdges = new List<HalfEdge>();
			m_Faces = new List<Face>();
		}

		public void AddEdge(double a_x, double a_y, double b_x, double b_y)
		{
			HalfEdge h1 = new HalfEdge();
			HalfEdge h2 = new HalfEdge();
			Edge e = new Edge();
			e.Half1 = h1;
			e.Half2 = h2;

			h1.Twin = h2;
			h2.Twin = h1;
			Vertex v1 = null;
			Vertex v2 = null;
			foreach (Vertex v in m_Vertices)
			{
				if (Math.Abs(v.X - a_x) <= float.Epsilon && Math.Abs(v.Y - a_y) <= float.Epsilon)
				{
					v1 = v;
				}
				else if (Math.Abs(v.X - b_x) <= float.Epsilon && Math.Abs(v.Y - b_y) <= float.Epsilon)
				{
					v2 = v;
				}
				if (v1 != null && v2 != null)
				{
					foreach (HalfEdge h in v1.IncidentEdges)
					{
						if (h.Twin.Origin == v2)
						{
							return;
						}
					}
					break;
				}
			}
			if (v1 == null)
			{
				v1 = new Vertex (a_x, a_y);
				m_Vertices.Add(v1);
			}
			if (v2 == null)
			{
				v2 = new Vertex(b_x, b_y);
				m_Vertices.Add(v2);
			}
			h1.Origin = v1;
			h2.Origin = v2;
			v1.IncidentEdges.Add(h1);
			v2.IncidentEdges.Add(h2);
			m_HalfEdges.Add(h1);
			m_HalfEdges.Add(h2);
			m_Edges.Add(e);
		}

		public void ConnectHalfEdges()
		{
			foreach (HalfEdge h in m_HalfEdges)
			{
				Vector3 edgeDirection = new Vector3((float)(h.Twin.Origin.X - h.Origin.X), (float)(h.Twin.Origin.Y - h.Origin.Y), 0);
				edgeDirection.Normalize();
				float turnSize = 0;
				HalfEdge mostLeftTurn = null;
				foreach (HalfEdge h2 in h.Twin.Origin.IncidentEdges)
				{
					if (h2 != h.Twin)
					{
						Vector3 nextEdgeDirection = new Vector3((float)(h2.Twin.Origin.X - h2.Origin.X), (float)(h2.Twin.Origin.Y - h2.Origin.Y), 0);
						nextEdgeDirection.Normalize();
						float turn = Vector3.Cross(edgeDirection, nextEdgeDirection).z;
						if (turn <= 0)
						{
							float size = Mathf.Abs(Vector3.Dot (edgeDirection, nextEdgeDirection) - 1);
							if (size >= turnSize)
							{
								mostLeftTurn = h2;
							}
						}
					}
				}
				if (mostLeftTurn != null)
				{
					h.Next = mostLeftTurn;
					mostLeftTurn.Previous = h;
				}
			}
		}

		public void CreateFaces()
		{
			List<HalfEdge> faceEdges = new List<HalfEdge>();
			foreach (HalfEdge h in m_HalfEdges)
			{
				if (h.IncidentFace == null)
				{
					faceEdges.Add(h);
					HalfEdge curEdge = h.Next;
					while (curEdge != h)
					{
						if (curEdge == null)
						{
							break;
						}
						else
						{
							faceEdges.Add(curEdge);
						}
						curEdge = curEdge.Next;
					}
					if (curEdge == h)
					{
						Face f = new Face();
						f.StartingEdge = h;
						foreach (HalfEdge newFaceEdge in faceEdges)
						{
							newFaceEdge.IncidentFace = f;
						}
						m_Faces.Add(f);
					}
					faceEdges.Clear();
				}
			}
		}

		public void AddVertexOnEdge(double a_x, double a_y, Edge a_Edge)
		{
			Vertex x = new Vertex(a_x, a_y);
			HalfEdge e1 = new HalfEdge();
			HalfEdge e2 = new HalfEdge();
			Edge e = new Edge();
			e.Half1 = e1;
			e.Half2 = e2;

			x.IncidentEdges.Add(e1);
			e1.Origin = x;

			e1.Next = a_Edge.Half1.Next;
			e1.Previous = e2;

			e1.IncidentFace = e1.IncidentFace;

			e2.Origin = a_Edge.UpperEndpoint;
			e2.Next = e1;
			e2.Previous = a_Edge.Half1.Previous;
			e2.IncidentFace = a_Edge.Half1.IncidentFace;

			a_Edge.Half1.Previous.Next = e2;
			a_Edge.Half1.Next.Previous = e1;

			m_HalfEdges.Remove(a_Edge.Half1);

			// Now the second halfedge.
			HalfEdge e3 = new HalfEdge();
			HalfEdge e4 = new HalfEdge();
			HalfEdge twinEdge = a_Edge.Half2;

			e3.Origin = twinEdge.Origin;
			e3.Next = e4;
			e3.Previous = twinEdge.Previous;
			e3.IncidentFace = twinEdge.IncidentFace;

			e4.Origin = x;
			e4.Next = twinEdge.Next;
			e4.Previous = e3;
			e4.IncidentFace = twinEdge.IncidentFace;

			twinEdge.Previous.Next = e3;
			twinEdge.Next.Previous = e4;

			e1.Twin = e3;
			e3.Twin = e1;
			e2.Twin = e4;
			e4.Twin = e2;

			m_HalfEdges.Remove(twinEdge);
			m_Edges.Remove(a_Edge);

			if (a_Edge.Half1.IncidentFace.StartingEdge == a_Edge.Half1)
			{
				a_Edge.Half1.IncidentFace.StartingEdge = e1;
			}
			if (twinEdge.IncidentFace.StartingEdge == twinEdge)
			{
				twinEdge.IncidentFace.StartingEdge = e3;
			}

			m_HalfEdges.Add(e1);
			m_HalfEdges.Add(e2);
			m_HalfEdges.Add(e3);
			m_HalfEdges.Add(e4);
			m_Edges.Add(e);
		}

		public void AddVertexInsideFace(double a_x, double a_y, HalfEdge a_h)
		{
			// Precondition: the open line segment (v, u), where u = target(h) = origin(twin(h))
			// and where v = (a_x, a_y) lies completely in f = face(h).

			Vertex v = new Vertex(a_x, a_y);
			HalfEdge h1 = new HalfEdge();
			HalfEdge h2 = new HalfEdge();
			Edge e = new Edge();
			e.Half1 = h1;
			e.Half2 = h2;

			v.IncidentEdges.Add(h2);
			h1.Twin = h2;
			h2.Twin = h1;
			h2.Origin = v;
			h1.Origin = a_h.Twin.Origin;

			h1.IncidentFace = a_h.IncidentFace;
			h2.IncidentFace = a_h.IncidentFace;

			h1.Next = h2;
			h2.Next = a_h.Next;

			h1.Previous = a_h;
			h2.Previous = h1;

			a_h.Next = h1;
			h2.Next.Previous = h2;

			m_Vertices.Add(v);
			m_HalfEdges.Add(h1);
			m_HalfEdges.Add(h2);
			m_Edges.Add(e);
		}

		public void SplitFace(HalfEdge a_h, Vertex a_v)
		{
			// Precondition: v is incident to f = face(h) but not adjacent to 
			// u = target(h) = origin(twin(h)). And the open line segment (v, u) lies
			// completely in f.

			Vertex u = a_h.Twin.Origin;
			Face f = a_h.IncidentFace;
			Face f1 = new Face();
			Face f2 = new Face();
			HalfEdge h1 = new HalfEdge();
			HalfEdge h2 = new HalfEdge();
			Edge e = new Edge();
			e.Half1 = h1;
			e.Half2 = h2;

			f1.StartingEdge = h1;
			f2.StartingEdge = h2;

			h1.Twin = h2;
			h2.Twin = h1;

			h2.Origin = a_v;
			h1.Origin = u;

			h2.Next = a_h.Next;
			h2.Next.Previous = h2;

			h1.Previous = a_h;
			a_h.Next = h1;

			HalfEdge i = h2;
			while (true)
			{
				i.IncidentFace = f2;
				if (i.Twin.Origin == a_v)
				{
					break;
				}
				else
				{
					i = i.Next;
				}
			}

			h1.Next = i.Next;
			h1.Next.Previous = h1;
			i.Next = h2;
			h2.Previous = i;
			i = h1;

			while (i.Twin.Origin != u)
			{
				i.IncidentFace = f1;
				i = i.Next;
			}

			m_Faces.Remove(f);
			m_Faces.Add(f1);
			m_Faces.Add(f2);

			m_HalfEdges.Add(h1);
			m_HalfEdges.Add(h2);
			m_Edges.Add(e);
		}

		public void FindIntersections()
		{
			AATree eventQueue = new AATree();
			foreach (Edge e in m_Edges)
			{
				eventQueue.Insert(e.UpperEndpoint);
				eventQueue.Insert(e.LowerEndpoint);
			}
			AATree status = new AATree();
			while (eventQueue.Size != 0)
			{
				Vertex p = eventQueue.FindMax();
				eventQueue.Delete(p);
				HandleEventPoint(p);
			}
		}

		private void HandleEventPoint(Vertex p)
		{
			// Implement this.
		}

		public void Draw()
		{
			GL.Begin(GL.LINES);
			GL.Color(Color.red);
			foreach (HalfEdge h in m_HalfEdges)
			{
				GL.Vertex3((float)(h.Origin.X), 0, (float)(h.Origin.Y));
				GL.Vertex3((float)(h.Twin.Origin.X), 0, (float)(h.Twin.Origin.Y));
			}
			GL.End();
		}
	}
}
