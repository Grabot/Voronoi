using System;

namespace VoronoiDCEL
{
	public sealed class HalfEdge
	{
		private Vertex m_Origin;
		private HalfEdge m_Twin;
		private Face m_IncidentFace; // the face to the left.
		private HalfEdge m_Next; // next halfedge on the boundary of IncidentFace.
		private HalfEdge m_Previous; // previous halfedge on the boundary of IncidentFace
		private Edge m_ParentEdge;

		public Vertex Origin
		{
			get { return m_Origin; }
			set { m_Origin = value; }
		}

		public HalfEdge Previous
		{
			get { return m_Previous; }
			set { m_Previous = value; }
		}

		public HalfEdge Next
		{
			get { return m_Next; }
			set { m_Next = value; }
		}

		public HalfEdge Twin
		{
			get { return m_Twin; }
			set { m_Twin = value; }
		}

		public Face IncidentFace
		{
			get { return m_IncidentFace; }
			set { m_IncidentFace = value; }
		}

		public Edge ParentEdge
		{
			get { return m_ParentEdge; }
			set { m_ParentEdge = value; }
		}
	}
}

