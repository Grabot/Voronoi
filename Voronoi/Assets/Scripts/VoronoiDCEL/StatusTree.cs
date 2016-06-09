namespace VoronoiDCEL
{
	public class StatusData : System.IComparable<StatusData>, System.IEquatable<StatusData>
	{
		public Edge edge;

		public override bool Equals(object obj)
		{
			if (obj != null && obj is StatusData)
			{
				return ((StatusData)obj).edge == edge;
			}
			else
			{
				return false;
			}
		}

		public bool Equals(StatusData a_StatusData)
		{
			if (a_StatusData != null)
			{
				return a_StatusData.edge == edge;
			}
			else
			{
				return false;
			}
		}

		public int CompareTo(StatusData a_StatusData)
		{
			return a_StatusData.edge.UpperEndpoint.CompareTo(edge) * -1;
		}

		public override int GetHashCode()
		{
			return edge.GetHashCode();
		}
	}

	public class StatusTree : AATree<StatusData>
	{
		public bool Insert(Edge a_Edge)
		{
			StatusData statusData = new StatusData();
			statusData.edge = a_Edge;
			return Insert(statusData);
		}

		protected override int CompareTo(StatusData a, StatusData b)
		{
			return a.CompareTo(b);
		}

		protected override bool IsEqual(StatusData a, StatusData b)
		{
			return a.Equals(b);
		}
	}
}
