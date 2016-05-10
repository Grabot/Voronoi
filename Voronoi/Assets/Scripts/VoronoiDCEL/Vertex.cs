using System;

namespace VoronoiDCEL
{
	public sealed class Vertex
	{
		private double m_x;
		private double m_y;
		private HalfEdge m_IncidentEdge; // IncidentEdge must have this vertex as origin.

		public Vertex(double a_x, double a_y)
		{
			m_x = a_x;
			m_y = a_y;
		}

		public HalfEdge IncidentEdge
		{
			get { return m_IncidentEdge; }
			set { m_IncidentEdge = value; }
		}

		public double X
		{ get { return m_x; } }

		public double Y
		{ get { return m_y; } }
	}
}

