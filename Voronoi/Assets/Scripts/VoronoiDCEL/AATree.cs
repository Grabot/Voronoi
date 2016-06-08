namespace VoronoiDCEL
{
	public class AATree<T> where T: System.IComparable<T>, System.IEquatable<T>
	{
		protected Node m_Bottom; // Sentinel.
		protected Node m_Deleted;
		protected Node m_Last;
		protected Node m_Tree;
		protected int m_Size = 0;

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

		public bool Insert(T data)
		{
			bool result = Insert(data, m_Tree);
			if (result)
			{
				++m_Size;
				return true;
			}
			return false;
		}

		public bool Delete(T data)
		{
			bool result = Delete(data, m_Tree);
			if (result)
			{
				--m_Size;
				return true;
			}
			return false;
		}

		public bool Contains(T data)
		{
			return Contains(data, m_Tree);
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

		private bool Insert(T data, Node t)
		{
			if (t == m_Bottom)
			{
				t = new Node();
				t.data = data;
				t.left = m_Bottom;
				t.right = m_Bottom;
				t.level = 1;
				return true;
			}
			else
			{
				bool result = false;
				if (data.CompareTo(m_Tree.data) < 0)
				{
					result = Insert(data, t.left);
				}
				else if (data.CompareTo(t.data) > 0)
				{
					result = Insert(data, t.right);
				}
				else
				{
					return false;
				}
				Skew(t);
				Split(t);
				return result;
			}
		}

		private bool Delete(T data, Node t)
		{
			if (t == m_Bottom)
			{
				return false;	
			}
			else
			{
				// Search down the tree and set pointers last and deleted.
				m_Last = t;
				bool result = false;
				if (data.CompareTo(t.data) < 0)
				{
					result = Delete(data, t.left);
				}
				else
				{
					m_Deleted = t;
					result = Delete(data, t.right);
				}

				// At the bottom of the tree we remove the element if it is present.
				if (t == m_Last && m_Deleted != m_Bottom && data.Equals(m_Deleted.data))
				{
					m_Deleted.data = t.data;
					m_Deleted = m_Bottom;
					t = t.right;
					m_Last = null;
					return true;
				}
				// On the way back, we rebalance.
				else if (t.left.level < t.level - 1 || t.right.level < t.level - 1)
				{
					--t.level;
					if (t.right.level > t.level)
					{
						t.right.level = t.level;
					}
					Skew(t);
					Skew(t.right);
					Skew(t.right.right);
					Split(t);
					Split(t.right);
				}
				return result;
			}
		}

		private bool Contains(T data, Node t)
		{
			if (t == m_Bottom)
			{
				return false;
			}
			else if (t.data.Equals(data))
			{
				return true;
			}
			else if (data.CompareTo(t.data) < 0)
			{
				return Contains(data, t.left);
			}
			else
			{
				return Contains(data, t.right);
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
	}
}