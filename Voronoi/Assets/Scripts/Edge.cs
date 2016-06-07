namespace VoronoiDCEL
{
	public class Edge
	{
		private HalfEdge m_Half1;
		private HalfEdge m_Half2;
		private Vertex m_UpperEndpoint;
		private Vertex m_LowerEndpoint;

		public HalfEdge Half1
		{
			get { return m_Half1; }
			set { m_Half1 = value; }
		}

		public HalfEdge Half2
		{
			get { return m_Half2; }
			set { m_Half2 = value; }
		}

		public Vertex UpperEndpoint 
		{
			get
			{ 
				if (m_UpperEndpoint != null)
				{
					return m_UpperEndpoint;
				}
				else
				{
					if (m_Half1.Origin < m_Half2.Origin)
					{
						m_LowerEndpoint = m_Half1.Origin;
						m_UpperEndpoint = m_Half2.Origin;
					}
					else
					{
						m_LowerEndpoint = m_Half2.Origin;
						m_UpperEndpoint = m_Half1.Origin;
					}
					return m_UpperEndpoint;
				}
			}
		}

		public Vertex LowerEndpoint 
		{
			get
			{ 
				if (m_LowerEndpoint != null)
				{
					return m_LowerEndpoint;
				}
				else
				{
					if (m_Half1.Origin < m_Half2.Origin)
					{
						m_LowerEndpoint = m_Half1.Origin;
						m_UpperEndpoint = m_Half2.Origin;
					}
					else
					{
						m_LowerEndpoint = m_Half2.Origin;
						m_UpperEndpoint = m_Half1.Origin;
					}
					return m_LowerEndpoint;
				}
			}
		}
	}
}
