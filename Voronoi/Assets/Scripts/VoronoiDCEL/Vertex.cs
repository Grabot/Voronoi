using System;
using System.Collections.Generic;

namespace VoronoiDCEL
{
	public sealed class Vertex
	{
		private double m_x;
		private double m_y;
		private List<HalfEdge> m_IncidentEdges; // IncidentEdges must have this vertex as origin.

		public Vertex(double a_x, double a_y)
		{
			m_x = a_x;
			m_y = a_y;
			m_IncidentEdges = new List<HalfEdge>();
		}

		public List<HalfEdge> IncidentEdges
		{
			get { return m_IncidentEdges; }
		}

		public double X
		{ get { return m_x; } }

		public double Y
		{ get { return m_y; } }

		public static bool operator <(Vertex a, Vertex b)
		{
			return a.Y < b.Y || (a.Y == b.Y && a.X < b.X);
		}

		public static bool operator >(Vertex a, Vertex b)
		{
			return a.Y > b.Y || (a.Y == b.Y && a.X > b.X);
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is Vertex)
			{
				return m_y == ((Vertex)obj).Y && m_x == ((Vertex)obj).X;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				hash = hash * 23 + m_x.GetHashCode();
				hash = hash * 23 + m_y.GetHashCode();
				return hash;
			}
		}
	}
}

