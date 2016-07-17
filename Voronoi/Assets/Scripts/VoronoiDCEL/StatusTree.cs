using System.Collections.Generic;
using System;

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

        public bool FindRightNeighbour(Edge a_Edge, out Edge out_RightNeighbour)
        {
            StatusData statusData = new StatusData(a_Edge);
            StatusData neighbour;
            if (FindRightNeighbour(statusData, out neighbour))
            {
                out_RightNeighbour = neighbour.Edge;
                return true;
            }
            out_RightNeighbour = null;
            return false;
        }

        public bool FindLeftNeighbour(Edge a_Edge, out Edge out_LeftNeighbour)
        {
            StatusData statusData = new StatusData(a_Edge);
            StatusData neighbour;
            if (FindLeftNeighbour(statusData, out neighbour))
            {
                out_LeftNeighbour = neighbour.Edge;
                return true;
            }
            out_LeftNeighbour = null;
            return false;
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
                throw new NotImplementedException("Find comparison not implemented.");
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
                throw new NotImplementedException("Find comparison not implemented.");
            }
        }

        protected bool IsEqual(Vertex v, StatusData b)
        {
            return b.Edge.UpperEndpoint == v || b.Edge.LowerEndpoint == v || v.OnLine(b.Edge);
        }

        protected int CompareTo(Vertex v, StatusData b)
        {
            return v.CompareTo(b.Edge);
        }

        public bool FindLeftNeighbour(Vertex v, out StatusData out_Neighbour)
        {
            Node result = FindNeighbour(v, m_Tree, false);
            if (result != null && result != m_Bottom)
            {
                out_Neighbour = result.Data;
                return true;
            }
            else
            {
                out_Neighbour = null;
                return false;
            }
        }

        public bool FindRightNeighbour(Vertex v, out StatusData out_Neighbour)
        {
            Node result = FindNeighbour(v, m_Tree, true);
            if (result != null && result != m_Bottom)
            {
                out_Neighbour = result.Data;
                return true;
            }
            else
            {
                out_Neighbour = null;
                return false;
            }
        }

        public bool FindLeftMostSegmentInSet(HashSet<Edge> a_Set, out Edge out_Leftmost)
        {
            return FindLeftMostSegmentInSet(a_Set, m_Tree, out out_Leftmost);
        }

        public bool FindRightMostSegmentInSet(HashSet<Edge> a_Set, out Edge out_Rightmost)
        {
            return FindRightMostSegmentInSet(a_Set, m_Tree, out out_Rightmost);
        }

        private Node FindNeighbour(Vertex v, Node t, bool rightNeighbour)
        {
            if (t == m_Bottom)
            {
                return null;
            }
            else
            {
                Queue<Node> qn = new Queue<Node>(m_Size);
                Queue<int> ql = new Queue<int>(m_Size);

                int level = 0;

                qn.Enqueue(m_Tree);
                ql.Enqueue(level);

                while (qn.Count != 0)
                {
                    Node node = qn.Dequeue();
                    level = ql.Dequeue();

                    if (IsEqual(v, node.Data))
                    {
                        if (ql.Count == 0 || ql.Peek() != level)
                        {
                            return null;
                        }
                        return qn.Peek();
                    }

                    if (rightNeighbour)
                    {
                        if (node.Left != m_Bottom)
                        {
                            qn.Enqueue(node.Left);
                            ql.Enqueue(level + 1);
                        }
                        if (node.Right != m_Bottom)
                        {
                            qn.Enqueue(node.Right);
                            ql.Enqueue(level + 1);
                        }
                    }
                    else
                    {
                        if (node.Right != m_Bottom)
                        {
                            qn.Enqueue(node.Right);
                            ql.Enqueue(level + 1);
                        }
                        if (node.Left != m_Bottom)
                        {
                            qn.Enqueue(node.Left);
                            ql.Enqueue(level + 1);
                        }
                    }
                }
                return null;
            }
        }

        private bool FindLeftMostSegmentInSet(HashSet<Edge> a_Set, Node t, out Edge out_LeftMost)
        {
            if (t == m_Bottom)
            {
                out_LeftMost = null;
                return false;
            }
            else
            {
                // Perform a DFS.
                Stack<Node> nodes = new Stack<Node>(m_Size);
                nodes.Push(t);
                while (nodes.Count != 0)
                {
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        if (curNode.Left == m_Bottom || a_Set.Count == 1)
                        {
                            out_LeftMost = curNode.Data.Edge;
                            return true;
                        }
                        else
                        {
                            a_Set.Remove(curNode.Data.Edge);
                            nodes.Push(curNode.Left);
                        }
                    }
                    else
                    {
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                        }
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                        }
                    }
                }
                out_LeftMost = null;
                return false;
            }
        }

        private bool FindRightMostSegmentInSet(HashSet<Edge> a_Set, Node t, out Edge out_Rightmost)
        {
            if (t == m_Bottom)
            {
                out_Rightmost = null;
                return false;
            }
            else
            {
                // Perform a DFS.
                Stack<Node> nodes = new Stack<Node>(m_Size);
                nodes.Push(t);
                while (nodes.Count != 0)
                {
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        if (curNode.Right == m_Bottom || a_Set.Count == 1)
                        {
                            out_Rightmost = curNode.Data.Edge;
                            return true;
                        }
                        else
                        {
                            a_Set.Remove(curNode.Data.Edge);
                            nodes.Push(curNode.Right);
                        }
                    }
                    else
                    {
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                        }
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                        }
                    }
                }
                out_Rightmost = null;
                return false;
            }
        }


    }
}
