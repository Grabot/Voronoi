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

		public bool Delete(Edge a_Edge)
		{
			StatusData statusData = new StatusData();
			statusData.edge = a_Edge;
			return Delete(statusData);
		}

		protected override int CompareTo(StatusData a, StatusData b, COMPARISON_TYPE a_ComparisonType)
		{
			if (a_ComparisonType == COMPARISON_TYPE.INSERT)
			{
				return a.edge.UpperEndpoint.CompareTo(b.edge);
			}
			else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
			{
				return a.edge.LowerEndpoint.CompareTo(b.edge);
			}
			else
			{
				throw new System.NotImplementedException("Find comparison not yet implemented.");
			}
		}

		protected override bool IsEqual(StatusData a, StatusData b, COMPARISON_TYPE a_ComparisonType)
		{
			if (a_ComparisonType == COMPARISON_TYPE.INSERT)
			{
				return a.edge.UpperEndpoint.OnLine(b.edge);
			}
			else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
			{
				return a.edge.Equals(b.edge);
			}
			else
			{
				throw new System.NotImplementedException("Find comparison not yet implemented.");
			}
		}
	}
}
