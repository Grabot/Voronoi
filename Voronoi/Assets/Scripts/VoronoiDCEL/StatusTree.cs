using System.Collections.Generic;

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

        public Edge[] FindNodes(Vertex v)
        {
            List<StatusData> nodes = new List<StatusData>();
            FindNodes(v, m_Tree, nodes);
            int nodeCount = nodes.Count;
            Edge[] edges = new Edge[nodeCount];
            for (int i = 0; i < nodeCount; ++i)
            {
                edges[i] = nodes[i].Edge;
            }
            return edges;
        }

        private void FindNodes(Vertex v, Node t, List<StatusData> list)
        {
            if (t == m_Bottom)
            {
                return;
            }
            else if (IsEqual(v, t.Data))
            {
                list.Add(t.Data);
                FindNodes(v, t.Left, list);
                FindNodes(v, t.Right, list);
            }
            else if (CompareTo(v, t.Data) < 0)
            {
                FindNodes(v, t.Left, list);
            }
            else
            {
                FindNodes(v, t.Right, list);
            }
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
                throw new System.NotImplementedException("Find comparison not implemented.");
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
                throw new System.NotImplementedException("Find comparison not implemented.");
            }
        }

        protected bool IsEqual(Vertex v, StatusData b)
        {
            return v.OnLine(b.Edge);
        }

        protected int CompareTo(Vertex v, StatusData b)
        {
            return v.CompareTo(b.Edge);
        }
    }
}
