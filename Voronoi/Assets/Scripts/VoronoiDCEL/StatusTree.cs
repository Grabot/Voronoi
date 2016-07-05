namespace VoronoiDCEL
{
    public class StatusData : System.IComparable<StatusData>, System.IEquatable<StatusData>
    {
        private readonly Edge m_Edge;

        public Edge Edge
        {
            get { return m_Edge; }
        }

        public StatusData(Edge a_Edge)
        {
            m_Edge = a_Edge;
        }

        public override bool Equals(object obj)
        {
            StatusData statusData = obj as StatusData;
            if (statusData != null)
            {
                return statusData.m_Edge == m_Edge;
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
                return a_StatusData.m_Edge == m_Edge;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(StatusData a_StatusData)
        {
            return a_StatusData.m_Edge.UpperEndpoint.CompareTo(m_Edge) * -1;
        }

        public override int GetHashCode()
        {
            return m_Edge.GetHashCode();
        }
    }

    public class StatusTree : AATree<StatusData>
    {
        public bool Insert(Edge a_Edge)
        {
            StatusData statusData = new StatusData(a_Edge);
            return Insert(statusData);
        }

        public bool Delete(Edge a_Edge)
        {
            StatusData statusData = new StatusData(a_Edge);
            return Delete(statusData);
        }

        protected override int CompareTo(StatusData a, StatusData b, COMPARISON_TYPE a_ComparisonType)
        {
            if (a_ComparisonType == COMPARISON_TYPE.INSERT)
            {
                return a.Edge.UpperEndpoint.CompareTo(b.Edge);
            }
            else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
            {
                return a.Edge.LowerEndpoint.CompareTo(b.Edge);
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
                return a.Edge.UpperEndpoint.OnLine(b.Edge);
            }
            else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
            {
                return a.Edge.Equals(b.Edge);
            }
            else
            {
                throw new System.NotImplementedException("Find comparison not yet implemented.");
            }
        }
    }
}
