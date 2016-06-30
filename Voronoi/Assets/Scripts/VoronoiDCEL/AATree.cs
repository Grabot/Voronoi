namespace VoronoiDCEL
{
    using System;
    using System.Collections.Generic;

    public class AATree<T> where T: System.IComparable<T>, System.IEquatable<T>
    {
        protected Node m_Bottom;
        // Sentinel.
        protected Node m_Deleted;
        protected Node m_Last;
        protected Node m_Tree;
        protected int m_Size = 0;

        protected internal class TraversalHistory
        {
            public TraversalHistory(Node a_Node, Node a_Parent, ECHILDSIDE a_ChildSide)
            {
                node = a_Node;
                parentNode = a_Parent;
                side = a_ChildSide;
            }

            public Node node;
            public Node parentNode;

            public enum ECHILDSIDE
            {
                LEFT,
                RIGHT,
                ISROOT
            }

            public ECHILDSIDE side;
        }

        protected enum COMPARISON_TYPE
        {
            INSERT,
            DELETE,
            FIND
        }

        public int Size
        {
            get { return m_Size; }
        }

        protected internal class Node
        {
            public Node left;
            public Node right;
            public int level;
            public T data;
        }

        public AATree()
        {
            m_Bottom = new Node();
            m_Bottom.level = 0;
            m_Bottom.left = m_Bottom;
            m_Bottom.right = m_Bottom;
            m_Deleted = m_Bottom;
            m_Tree = m_Bottom;
        }

        public bool Contains(T data)
        {
            return FindNode(data, m_Tree) != null;
        }

        public int ComputeSize()
        {
            return ComputeSize(m_Tree);
        }

        public bool FindMin(out T out_MinValue)
        {
            Node min = FindMinNode();
            if (min != m_Bottom)
            {
                out_MinValue = min.data;
                return true;
            }
            out_MinValue = default(T);
            return false;
        }

        public bool FindMax(out T out_MaxValue)
        {
            Node max = FindMaxNode();
            if (max != m_Bottom)
            {
                out_MaxValue = max.data;
                return true;
            }
            out_MaxValue = default(T);
            return false;
        }

        protected Node FindMinNode()
        {
            Node min = m_Tree;
            while (min.left != m_Bottom)
            {
                min = min.left;
            }
            return min;
        }

        protected Node FindMaxNode()
        {
            Node max = m_Tree;
            while (max.right != m_Bottom)
            {
                max = max.right;
            }
            return max;
        }

        public bool DeleteMax()
        {
            T max;
            return FindMax(out max) && Delete(max);
        }

        public bool DeleteMin()
        {
            T min;
            return FindMin(out min) && Delete(min);
        }

        private void Skew(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.left.level == t.level)
            {
                // Rotate right.
                Node oldLeft = t.left;
                Node newLeft = oldLeft.right;
                t.left = newLeft;
                oldLeft.right = t;
                if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
                {
                    parent.left = oldLeft;
                }
                else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
                {
                    parent.right = oldLeft;
                }
                else
                {
                    m_Tree = oldLeft;
                }

                //Node temp = t;
                //t = t.left;
                //temp.left = t.right;
                //t.right = temp;
            }
        }

        private void Split(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.right.right.level == t.level)
            {
                // Rotate left.
                Node oldRight = t.right;
                Node newRight = oldRight.left;
                t.right = newRight;
                oldRight.left = t;
                if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
                {
                    parent.left = oldRight;
                }
                else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
                {
                    parent.right = oldRight;
                }
                else
                {
                    m_Tree = oldRight;
                }
                ++oldRight.level;

                //Node temp = t;
                //t = t.right;
                //temp.right = t.left;
                //t.left = temp;
                //++t.level;
            }
        }

        public bool Insert(T data)
        {
            if (!(data is ValueType) && EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return false;
            }
            else if (m_Tree == m_Bottom)
            {
                m_Tree = CreateNode(data);
                return true;
            }
            else
            {
                Node currentNode = m_Tree;
                Node parent = null;
                int comparisonResult = 0;
                Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
                nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));
                while (currentNode != m_Bottom)
                {
                    parent = currentNode;
                    comparisonResult = CompareTo(data, currentNode.data, COMPARISON_TYPE.INSERT);
                    TraversalHistory histEntry;
                    if (comparisonResult < 0)
                    {
                        currentNode = currentNode.left;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                    }
                    else if (comparisonResult > 0)
                    {
                        currentNode = currentNode.right;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                    }
                    else
                    {
                        return false;
                    }
                    nodeStack.Push(histEntry);
                }
                nodeStack.Pop(); // This node is m_Bottom.

                if (comparisonResult < 0)
                {
                    parent.left = CreateNode(data);
                }
                else if (comparisonResult > 0)
                {
                    parent.right = CreateNode(data);
                }

                while (nodeStack.Count != 0)
                {
                    TraversalHistory t = nodeStack.Pop();
                    Skew(t.node, t.parentNode, t.side);
                    Split(t.node, t.parentNode, t.side);
                }
                return true;
            }
        }

        private Node CreateNode(T data)
        {
            Node n = new Node();
            n.data = data;
            n.left = m_Bottom;
            n.right = m_Bottom;
            n.level = 1;
            ++m_Size;
            return n;
        }

        public bool Delete(T data)
        {
            if (m_Tree == m_Bottom || EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return false;
            }
            else
            {
                Node currentNode = m_Tree;
                Node parent = null;
                Node deleted = m_Bottom;
                Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
                nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));
                while (currentNode != m_Bottom)
                {
                    parent = currentNode;
                    TraversalHistory hist;
                    int comparisonResult = CompareTo(data, currentNode.data, COMPARISON_TYPE.DELETE);
                    if (comparisonResult < 0)
                    {
                        currentNode = currentNode.left;
                        hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                    }
                    else
                    {
                        deleted = currentNode;
                        currentNode = currentNode.right;
                        hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                    }
                    nodeStack.Push(hist);
                }

                if (deleted != m_Bottom && CompareTo(data, deleted.data, COMPARISON_TYPE.DELETE) == 0)
                {
                    nodeStack.Pop(); // Pop since the last entry is m_Bottom
                    Node last = nodeStack.Pop().node; // This is the node that is leftmost of the node that we want to delete.
                    deleted.data = last.data;
                    Node copy = last.right;
                    last.data = copy.data;
                    last.left = copy.left;
                    last.right = copy.right;
                    last.level = copy.level;
                    // Destroy the node
                    copy.data = default(T);
                    copy.left = null;
                    copy.right = null;
                    --m_Size;
                }
                else
                {
                    return false;
                }

                while (nodeStack.Count != 0)
                {
                    TraversalHistory t = nodeStack.Pop();
                    Node n = t.node;
                    if (n.left.level < n.level - 1 || n.right.level < n.level - 1)
                    {
                        --n.level;
                        if (n.right.level > n.level)
                        {
                            n.right.level = n.level;
                        }
                        Skew(n, t.parentNode, t.side);
                        Skew(n.right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                        Skew(n.right.right, n.right, TraversalHistory.ECHILDSIDE.RIGHT);
                        Split(n, t.parentNode, t.side);
                        Split(n.right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                    }
                }
                return true;
            }
        }

        private Node FindNode(T data, Node t)
        {
            if (t == m_Bottom)
            {
                return null;
            }
            else if (IsEqual(t.data, data, COMPARISON_TYPE.FIND))
            {
                return t;
            }
            else if (CompareTo(data, t.data, COMPARISON_TYPE.FIND) < 0)
            {
                return FindNode(data, t.left);
            }
            else
            {
                return FindNode(data, t.right);
            }
        }

        private int ComputeSize(Node t)
        {
            if (t == m_Bottom)
            {
                return 0;
            }
            else
            {
                int result = 1;
                result += ComputeSize(t.left);
                result += ComputeSize(t.right);
                return result;
            }
        }

        protected virtual int CompareTo(T a, T b, COMPARISON_TYPE a_ComparisonType)
        {
            return a.CompareTo(b);
        }

        protected virtual bool IsEqual(T a, T b, COMPARISON_TYPE a_ComparisonType)
        {
            return a.Equals(b);
        }
    }
}