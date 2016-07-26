using System.Collections.Generic;
using System;

namespace VoronoiDCEL
{
    public class StatusData : IComparable<StatusData>, IEquatable<StatusData>
    {
        private readonly Edge m_Edge;
        private readonly Vertex m_SweeplineIntersectionPoint;
        private readonly Vertex m_PointP;

        public Edge Edge
        {
            get { return m_Edge; }
        }

        public Vertex SweeplineIntersectionPoint
        {
            get { return m_SweeplineIntersectionPoint; }
        }

        public Vertex PointP
        {
            get { return m_PointP; }
        }

        public StatusData(Edge a_Edge)
        {
            m_Edge = a_Edge;
            m_SweeplineIntersectionPoint = Vertex.Zero;
            m_PointP = Vertex.Zero;
        }

        public StatusData(Edge a_Edge, Vertex a_PointP)
        {
            m_Edge = a_Edge;
            m_PointP = a_PointP;
            double sweeplineHeight = a_PointP.Y;
            if (!DCEL.IntersectLines(a_Edge.UpperEndpoint, a_Edge.LowerEndpoint, new Vertex(Math.Min(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) - 1, sweeplineHeight),
                    new Vertex(Math.Max(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) + 1, sweeplineHeight), out m_SweeplineIntersectionPoint))
            {
                m_SweeplineIntersectionPoint = a_Edge.UpperEndpoint;
            }
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
        public bool Insert(Edge a_Edge, Vertex a_PointP)
        {
            StatusData statusData = new StatusData(a_Edge, a_PointP);
            return Insert(statusData);
        }

        public bool Delete(Edge a_Edge, Vertex a_PointP)
        {
            StatusData statusData = new StatusData(a_Edge, a_PointP);
            return Delete(statusData);
        }

        public bool FindNextBiggest(Edge a_Edge, Vertex a_PointP, out Edge out_NextBiggest)
        {
            StatusData statusData = new StatusData(a_Edge, a_PointP);
            StatusData nextBiggest;
            if (FindNextBiggest(statusData, out nextBiggest))
            {
                out_NextBiggest = nextBiggest.Edge;
                return true;
            }
            out_NextBiggest = null;
            return false;
        }

        public bool FindNextSmallest(Edge a_Edge, Vertex a_PointP, out Edge out_NextSmallest)
        {
            StatusData statusData = new StatusData(a_Edge, a_PointP);
            StatusData nextSmallest;
            if (FindNextSmallest(statusData, out nextSmallest))
            {
                out_NextSmallest = nextSmallest.Edge;
                return true;
            }
            out_NextSmallest = null;
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
            // If the edge we're comparing with is the one we're trying to delete or find, then return 0.
            if (a_ComparisonType != COMPARISON_TYPE.INSERT &&
                a.Edge.LowerEndpoint.Equals(b.Edge.LowerEndpoint) &&
                a.Edge.UpperEndpoint.Equals(b.Edge.UpperEndpoint))
            {
                return 0;
            }

            // Is the edge we're inserting/deleting horizontal?
            if (a.Edge.IsHorizontal)
            {
                // Is the edge we're comparing with also horizontal?
                if (b.Edge.IsHorizontal)
                {
                    // Check if the two horizontal edges overlap; this is invalid.
                    if (a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ||
                        b.Edge.LowerEndpoint.X <= a.Edge.UpperEndpoint.X)
                    {
                        // The two horizontal edges don't overlap, so return if it's left or right.
                        return a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X ? -1 : 1;
                    }
                    else
                    {
                        throw new Exception("Horizontal edges cannot overlap!");
                    }
                }
                else
                {
                    // Edge b is not horizontal, but a is, so compare using a's lower endpoint.
                    // If the lower endpoint is to the left or on b then the whole of a is to the left.
                    // But if the lower endpoint is to the right of b, then either a is intersecting b
                    // or the whole of a is on the right of b. Horizontal segments always come last among segments
                    // that are intersecting p, so in both cases we can return that it's to the right.
                    if (a_ComparisonType != COMPARISON_TYPE.DELETE && a.PointP.CompareTo(b.Edge) == 0 && !b.Edge.LowerEndpoint.Equals(a.PointP) && !b.Edge.UpperEndpoint.Equals(a.PointP))
                    {
                        return 1;
                    }
                    if (a_ComparisonType == COMPARISON_TYPE.DELETE && a.Edge.LowerEndpoint.Equals(a.PointP) && a.Edge.LowerEndpoint.CompareTo(b.Edge) > 0)
                    {
                        return 1;
                    }
                    return a.Edge.UpperEndpoint.CompareTo(b.Edge) < 0 ? -1 : 1;
                }
            }
            // a is not horizontal. Is the edge we're comparing with horizontal?
            else if (b.Edge.IsHorizontal)
            {
                // If the point on a that is intersecting with the sweep line (a.Vertex) is
                // to the left of b's lower endpoint, then either we are on the left or intersecting.
                // In both cases we return left, because horizontal segments always come last.
                if (a_ComparisonType != COMPARISON_TYPE.DELETE && a.PointP.CompareTo(a.Edge) == 0 && !a.Edge.LowerEndpoint.Equals(a.PointP) && !a.Edge.UpperEndpoint.Equals(a.PointP))
                {
                    return -1;
                }
                return a.SweeplineIntersectionPoint.X <= b.Edge.UpperEndpoint.X ? -1 : 1;
            }
            else
            {
                // Both a and b are not horizontal. The easy case! Use a.Vertex (the point of a intersecting the sweep line)
                // and look on what side of b it is.
                int side = a.SweeplineIntersectionPoint.CompareTo(b.Edge);
                if (side == 0)
                {
                    // If we're deleting and the point p is a's lower endpoint, then we cannot use a's
                    // lower endpoint to resolve this case, so we must use the upper endpoint.
                    if (a_ComparisonType == COMPARISON_TYPE.DELETE && a.SweeplineIntersectionPoint.Equals(a.Edge.LowerEndpoint))
                    {
                        side = a.Edge.UpperEndpoint.CompareTo(b.Edge);
                    }
                    else
                    {
                        // a.Vertex was on b, so use a's lower endpoint to check on what side it is.
                        side = a.Edge.LowerEndpoint.CompareTo(b.Edge);
                    }
                    if (side == 0)
                    {
                        // a's lower endpoint is also on b, this should not occur in a planar subdivision!
                        throw new Exception("Edges cannot overlap!");
                    }
                }
                return side;
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
