using System.Collections.Generic;
using System;

namespace VoronoiDCEL
{
    public class StatusData : IComparable<StatusData>, IEquatable<StatusData>
    {
        private readonly Edge m_Edge;
        private readonly Vertex m_Vertex;

        public Edge Edge
        {
            get { return m_Edge; }
        }

        public Vertex Vertex
        {
            get { return m_Vertex; }
        }

        public StatusData(Edge a_Edge)
        {
            m_Edge = a_Edge;
            m_Vertex = Vertex.Zero;
        }

        public StatusData(Edge a_Edge, Vertex a_Vertex)
        {
            m_Edge = a_Edge;
            m_Vertex = a_Vertex;
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
        public bool Insert(Edge a_Edge, double a_SweepLineHeight)
        {
            Vertex intersection;
            if (!DCEL.IntersectLines(a_Edge.UpperEndpoint, a_Edge.LowerEndpoint, new Vertex(Math.Min(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) - 1, a_SweepLineHeight),
                    new Vertex(Math.Max(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) + 1, a_SweepLineHeight), out intersection))
            {
                intersection = a_Edge.UpperEndpoint;
            }
            StatusData statusData = new StatusData(a_Edge, intersection);
            return Insert(statusData);
        }

        public bool Delete(Edge a_Edge, double a_SweepLineHeight)
        {
            Vertex intersection;
            if (!DCEL.IntersectLines(a_Edge.UpperEndpoint, a_Edge.LowerEndpoint, new Vertex(Math.Min(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) - 1, a_SweepLineHeight),
                    new Vertex(Math.Max(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) + 1, a_SweepLineHeight), out intersection))
            {
                intersection = a_Edge.UpperEndpoint;
            }
            StatusData statusData = new StatusData(a_Edge, intersection);
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
                if (Math.Abs(a.Edge.LowerEndpoint.Y - a.Edge.UpperEndpoint.Y) <= double.Epsilon)
                {
                    if (Math.Abs(b.Edge.LowerEndpoint.Y - b.Edge.UpperEndpoint.Y) <= double.Epsilon)
                    {
                        if (a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ||
                            b.Edge.LowerEndpoint.X <= a.Edge.UpperEndpoint.X)
                        {
                            return a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ? -1 : 1;
                        }
                        else
                        {
                            throw new Exception("Horizontal edges cannot overlap!");
                        }
                    }
                    else
                    {
                        return a.Edge.LowerEndpoint.CompareTo(b.Edge) < 0 ? -1 : 1;
                    }
                }
                else
                {
                    int side = a.Vertex.CompareTo(b.Edge);
                    if (side == 0)
                    {
                        side = a.Edge.LowerEndpoint.CompareTo(b.Edge);
                        if (side == 0)
                        {
                            throw new Exception("Edges cannot overlap!");
                        }
                    }
                    return side;
                }
            }
            else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
            {
                if (a.Edge.LowerEndpoint.Equals(b.Edge.LowerEndpoint) &&
                    a.Edge.UpperEndpoint.Equals(b.Edge.UpperEndpoint))
                {
                    return 0;
                }
                else if (Math.Abs(a.Edge.LowerEndpoint.Y - a.Edge.UpperEndpoint.Y) <= double.Epsilon)
                {
                    if (Math.Abs(b.Edge.LowerEndpoint.Y - b.Edge.UpperEndpoint.Y) <= double.Epsilon)
                    {
                        if (a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ||
                            b.Edge.LowerEndpoint.X <= a.Edge.UpperEndpoint.X)
                        {
                            return a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ? -1 : 1;
                        }
                        else
                        {
                            throw new Exception("Horizontal edges cannot overlap!");
                        }
                    }
                    else
                    {
                        return a.Edge.LowerEndpoint.CompareTo(b.Edge) < 0 ? -1 : 1;
                    }
                }
                else if (Math.Abs(b.Edge.LowerEndpoint.Y - b.Edge.UpperEndpoint.Y) <= double.Epsilon)
                {
                    return a.Vertex.X > b.Edge.LowerEndpoint.X ? 1 : -1;
                }
                else
                {
                    int side = a.Vertex.CompareTo(b.Edge);
                    if (side == 0)
                    {
                        side = a.Edge.UpperEndpoint.CompareTo(b.Edge);
                        if (side == 0)
                        {
                            if (a.Edge.UpperEndpoint.Equals(b.Edge.UpperEndpoint) &&
                                a.Edge.LowerEndpoint.Equals(b.Edge.LowerEndpoint))
                            {
                                return 0;
                            }
                            else
                            {
                                throw new Exception("Edges cannot overlap!");
                            }
                        }
                    }
                    return side;
                }
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
                return a.Edge.UpperEndpoint.OnLine(b.Edge) && a.Edge.LowerEndpoint.OnLine(b.Edge);
            }
            else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
            {
                return a.Edge.Equals(b.Edge);
            }
            else
            {
                return a.Equals(b);
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
                out_LeftMost = null;
                // Perform a DFS.
                int setCount = a_Set.Count;
                Stack<Node> nodes = new Stack<Node>(m_Size);
                Stack<int> nodesLevel = new Stack<int>(m_Size);
                nodes.Push(t);
                nodesLevel.Push(0);
                int lowestLevel = -1;
                while (nodes.Count != 0 && setCount != 0)
                {
                    int curLevel = nodesLevel.Pop();
                    if (curLevel <= lowestLevel && out_LeftMost != null)
                    {
                        return true;
                    }
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        --setCount;
                        if (out_LeftMost == null)
                        {
                            if (curNode.Left == m_Bottom || setCount == 0)
                            {
                                out_LeftMost = curNode.Data.Edge;
                                return true;
                            }
                            else
                            {
                                out_LeftMost = curNode.Data.Edge;
                                lowestLevel = curLevel;
                                nodes.Push(curNode.Left);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                        else
                        {
                            out_LeftMost = curNode.Data.Edge;
                            lowestLevel = curLevel;
                            if (curNode.Left != m_Bottom)
                            {
                                nodes.Push(curNode.Left);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                    }
                    else
                    {
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                            nodesLevel.Push(curLevel + 1);
                        }
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                            nodesLevel.Push(curLevel + 1);
                        }
                    }
                }
                return out_LeftMost != null;
            }
        }

        private bool FindRightMostSegmentInSet(HashSet<Edge> a_Set, Node t, out Edge out_RightMost)
        {
            if (t == m_Bottom)
            {
                out_RightMost = null;
                return false;
            }
            else
            {
                out_RightMost = null;
                // Perform a DFS.
                int setCount = a_Set.Count;
                Stack<Node> nodes = new Stack<Node>(m_Size);
                Stack<int> nodesLevel = new Stack<int>(m_Size);
                nodes.Push(t);
                nodesLevel.Push(0);
                int lowestLevel = -1;
                while (nodes.Count != 0 && setCount != 0)
                {
                    int curLevel = nodesLevel.Pop();
                    if (curLevel <= lowestLevel && out_RightMost != null)
                    {
                        return true;
                    }
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        --setCount;
                        if (out_RightMost == null)
                        {
                            if (curNode.Right == m_Bottom || setCount == 0)
                            {
                                out_RightMost = curNode.Data.Edge;
                                return true;
                            }
                            else
                            {
                                out_RightMost = curNode.Data.Edge;
                                lowestLevel = curLevel;
                                nodes.Push(curNode.Right);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                        else
                        {
                            out_RightMost = curNode.Data.Edge;
                            lowestLevel = curLevel;
                            if (curNode.Right != m_Bottom)
                            {
                                nodes.Push(curNode.Right);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                    }
                    else
                    {
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                            nodesLevel.Push(curLevel + 1);
                        }
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                            nodesLevel.Push(curLevel + 1);
                        }
                    }
                }
                return out_RightMost != null;
            }
        }


    }
}
