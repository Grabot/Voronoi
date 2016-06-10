namespace VoronoiDCEL
{
	using System;
	using System.Collections.Generic;

	public class AATree<T> where T: System.IComparable<T>, System.IEquatable<T>
	{
		protected Node m_Bottom; // Sentinel.
		protected Node m_Deleted;
		protected Node m_Last;
		protected Node m_Tree;
		protected int m_Size = 0;
		protected enum COMPARISON_TYPE { INSERT, DELETE, FIND }

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

		public T FindMin()
		{
			Node min = m_Tree;
			while (min.left != m_Bottom)
			{
				min = min.left;
			}
			return min.data;
		}

		public T FindMax()
		{
			Node max = m_Tree;
			while (max.right != m_Bottom)
			{
				max = max.right;
			}
			return max.data;
		}

		public bool DeleteMax()
		{
			T max = FindMax();
			if (max != null)
			{
				return Delete(max);
			}
			else
			{
				return false;
			}
		}

		public bool DeleteMin()
		{
			T min = FindMin();
			if (min != null)
			{
				return Delete(min);
			}
			else
			{
				return false;
			}
		}

		private void Skew(Node t)
		{
			if (t.left.level == t.level)
			{
				// Rotate right.
				Node temp = t;
				t = t.left;
				temp.left = t.right;
				t.right = temp;
			}
		}

		private void Split(Node t)
		{
			if (t.right.right.level == t.level)
			{
				// Rotate left.
				Node temp = t;
				t = t.right;
				temp.right = t.left;
				t.left = temp;
				++t.level;
			}
		}

		public bool Insert(T data)
		{
			if (data == null)
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
				Stack<Node> nodeStack = new Stack<Node>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
				while (currentNode != m_Bottom)
				{
					parent = currentNode;
					comparisonResult = CompareTo(data, currentNode.data, COMPARISON_TYPE.INSERT);
					if (comparisonResult < 0)
					{
						currentNode = currentNode.left;
					}
					else if (comparisonResult > 0)
					{
						currentNode = currentNode.right;
					}
					else
					{
						return false;
					}
					nodeStack.Push(parent);
				}

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
					Node t = nodeStack.Pop();
					Skew(t);
					Split(t);
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
			if (m_Tree == m_Bottom || data == null)
			{
				return false;
			}
			else
			{
				Node currentNode = m_Tree;
				Node parent = null;
				Node deleted = m_Bottom;
				Stack<Node> nodeStack = new Stack<Node>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
				while (currentNode != m_Bottom)
				{
					parent = currentNode;
					int comparisonResult = CompareTo(data, currentNode.data, COMPARISON_TYPE.DELETE);
					if (comparisonResult < 0)
					{
						currentNode = currentNode.left;
					}
					else
					{
						deleted = currentNode;
						currentNode = currentNode.right;
					}
					nodeStack.Push(parent);
				}

				if (deleted != m_Bottom && CompareTo(data, deleted.data, COMPARISON_TYPE.DELETE) == 0)
				{
					deleted.data = parent.data;
					Node copy = parent.right;
					parent.data = copy.data;
					parent.left = copy.left;
					parent.right = copy.right;
					parent.level = copy.level;
					nodeStack.Pop();
					--m_Size;
				}
				else
				{
					return false;
				}

				while (nodeStack.Count != 0)
				{
					Node n = nodeStack.Pop();
					if (n.left.level < n.level - 1 || n.right.level < n.level - 1)
					{
						--n.level;
						if (n.right.level > n.level)
						{
							n.right.level = n.level;
						}
						Skew(n);
						Skew(n.right);
						Skew(n.right.right);
						Split(n);
						Split(n.right);
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