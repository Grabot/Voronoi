namespace VoronoiDCEL
{
	public class Edge : System.IComparable<Edge>, System.IEquatable<Edge>
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

		public override bool Equals(object obj)
		{
			if (obj != null && obj is Edge)
			{
				return (((Edge)obj).Half1 == m_Half1 && ((Edge)obj).Half2 == m_Half2) ||
					   (((Edge)obj).Half1 == m_Half2 && ((Edge)obj).Half2 == m_Half1);
			}
			else
			{
				return false;
			}
		}

		public bool Equals(Edge a_Edge)
		{
			if (a_Edge != null)
			{
				return (a_Edge.Half1 == m_Half1 && a_Edge.Half2 == m_Half2) ||
					(a_Edge.Half1 == m_Half2 && a_Edge.Half2 == m_Half1);
			}
			else
			{
				return false;
			}
		}

		public int CompareTo(Edge a_Edge)
		{
			throw new System.NotImplementedException();
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				hash = hash * 23 + m_Half1.GetHashCode();
				hash = hash * 23 + m_Half2.GetHashCode();
				return hash;
			}
		}
	}
}
