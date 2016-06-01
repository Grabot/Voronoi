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
	}
}

