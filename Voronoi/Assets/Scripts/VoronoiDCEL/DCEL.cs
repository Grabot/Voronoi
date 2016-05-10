using System;
using System.Collections.Generic;

namespace VoronoiDCEL
{
	public class DCEL
	{
		private List<Vertex> m_Vertices;
		private List<HalfEdge> m_HalfEdges;
		private List<Face> m_Faces;

		public DCEL()
		{
			m_Vertices = new List<Vertex>();
			m_HalfEdges = new List<HalfEdge>();
			m_Faces = new List<Face>();
		}

		public void AddVertexOnEdge(double a_x, double a_y, HalfEdge a_Edge)
		{
			Vertex x = new Vertex(a_x, a_y);
			HalfEdge e1 = new HalfEdge();
			HalfEdge e2 = new HalfEdge();

			x.IncidentEdge = e1;
			e1.Origin = x;

			e1.Next = a_Edge.Next;
			e1.Previous = e2;

			e1.IncidentFace = e1.IncidentFace;

			e2.Origin = a_Edge.Origin;
			e2.Next = e1;
			e2.Previous = a_Edge.Previous;
			e2.IncidentFace = a_Edge.IncidentFace;

			a_Edge.Previous.Next = e2;
			a_Edge.Next.Previous = e1;

			m_HalfEdges.Remove(a_Edge);

			// Now the second halfedge.
			HalfEdge e3 = new HalfEdge();
			HalfEdge e4 = new HalfEdge();
			HalfEdge twinEdge = a_Edge.Twin;

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

			if (a_Edge.IncidentFace.StartingEdge == a_Edge)
			{
				a_Edge.IncidentFace.StartingEdge = e1;
			}
			if (twinEdge.IncidentFace.StartingEdge == twinEdge)
			{
				twinEdge.IncidentFace.StartingEdge = e3;
			}
		}

		public void AddVertexInsideFace(double a_x, double a_y, HalfEdge a_h)
		{
			// Precondition: the open line segment (v, u), where u = target(h) = origin(twin(h))
			// and where v = (a_x, a_y) lies completely in f = face(h).

			Vertex v = new Vertex(a_x, a_y);
			HalfEdge h1 = new HalfEdge();
			HalfEdge h2 = new HalfEdge();

			v.IncidentEdge = h2;
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
		}

		public void SplitFace(HalfEdge a_h, Vertex a_v)
		{
			// Precondition: v is incident to f = face(h) but not adjacent to 
			// u = target(h) = origin(twin(h)). And the open line segment (v, u) lies
			// completely in f.

			Vertex u = a_h.Twin.Origin;
			Face f = a_h.IncidentFace;
			Face f1 = new Face ();
			Face f2 = new Face ();
			HalfEdge h1 = new HalfEdge ();
			HalfEdge h2 = new HalfEdge ();

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
		}
	}
}

