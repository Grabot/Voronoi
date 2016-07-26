﻿namespace VoronoiDCEL
{
    using System;

    public class Edge : IComparable<Edge>, IEquatable<Edge>
    {
        private HalfEdge m_Half1;
        private HalfEdge m_Half2;
        private Vertex m_UpperEndpoint;
        private Vertex m_LowerEndpoint;

        private enum E_ISHORIZONTAL
        {
            YES,
            NO,
            DONTKNOW
        }

        private E_ISHORIZONTAL m_IsHorizontal = E_ISHORIZONTAL.DONTKNOW;

        public HalfEdge Half1
        {
            get { return m_Half1; }
            set
            {
                m_Half1 = value;
                m_IsHorizontal = E_ISHORIZONTAL.DONTKNOW;
            }
        }

        public HalfEdge Half2
        {
            get { return m_Half2; }
            set
            {
                m_Half2 = value;
                m_IsHorizontal = E_ISHORIZONTAL.DONTKNOW;
            }
        }

        public bool IsHorizontal
        {
            get
            {
                if (m_IsHorizontal == E_ISHORIZONTAL.DONTKNOW)
                {
                    m_IsHorizontal = ComputeHorizontal();
                }
                switch (m_IsHorizontal)
                {
                    case E_ISHORIZONTAL.YES:
                        return true;
                    case E_ISHORIZONTAL.NO:
                        return false;
                    case E_ISHORIZONTAL.DONTKNOW:
                        throw new Exception("Unable to compute if edge is horizontal");
                    default:
                        throw new Exception("Unable to compute if edge is horizontal");
                }
            }
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
                    if (m_Half1.Origin.Y > m_Half2.Origin.Y ||
                        (Math.Abs(m_Half1.Origin.Y - m_Half2.Origin.Y) <= double.Epsilon && m_Half1.Origin.X < m_Half2.Origin.X))
                    {
                        m_UpperEndpoint = m_Half1.Origin;
                        m_LowerEndpoint = m_Half2.Origin;
                    }
                    else
                    {
                        m_UpperEndpoint = m_Half2.Origin;
                        m_LowerEndpoint = m_Half1.Origin;
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
                    if (m_Half1.Origin.Y < m_Half2.Origin.Y ||
                        (Math.Abs(m_Half1.Origin.Y - m_Half2.Origin.Y) <= double.Epsilon && m_Half1.Origin.X > m_Half2.Origin.X))
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

        private E_ISHORIZONTAL ComputeHorizontal()
        {
            if (m_Half1 != null && m_Half2 != null)
            {
                return (Math.Abs(m_Half1.Origin.Y - m_Half2.Origin.Y) <= double.Epsilon) ? E_ISHORIZONTAL.YES : E_ISHORIZONTAL.NO;
            }
            else
            {
                return E_ISHORIZONTAL.DONTKNOW;
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
            throw new NotImplementedException();
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

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("Lower endpoint: ");
            builder.Append(m_LowerEndpoint.ToString());
            builder.Append(" Upper endpoint: ");
            builder.Append(m_UpperEndpoint.ToString());
            builder.Append(" Is horizontal: ");
            builder.Append(m_IsHorizontal.ToString());
            return builder.ToString();
        }
    }
}
