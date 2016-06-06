namespace VoronoiDCEL
{
	public class AATree
	{
		Node m_Bottom; // Sentinel.
		Node m_Deleted;
		Node m_Last;
		Node m_Tree;

		internal class Node
		{
			public Node left;
			public Node right;
			public int level;
			public Vertex data;
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

		public bool Insert(Vertex data)
		{
			return Insert(data, m_Tree);
		}

		public bool Delete(Vertex data)
		{
			return Delete(data, m_Tree);
		}

		public bool Contains(Vertex data)
		{
			return Contains(data, m_Tree);
		}

		public Vertex FindMin()
		{
			Node min = m_Tree;
			while (min.left != m_Bottom)
			{
				min = min.left;
			}
			return min.data;
		}

		public Vertex FindMax()
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
			Vertex max = FindMax();
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
			Vertex min = FindMin();
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

		private bool Insert(Vertex data, Node t)
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
				if (data < m_Tree.data)
				{
					result = Insert(data, t.left);
				}
				else if (data > t.data)
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

		private bool Delete(Vertex data, Node t)
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
				if (data < t.data)
				{
					result = Delete(data, t.left);
				}
				else
				{
					m_Deleted = t;
					result = Delete(data, t.right);
				}

				// At the bottom of the tree we remove the element if it is present.
				if (t == m_Last && m_Deleted != m_Bottom && data == m_Deleted.data)
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

		private bool Contains(Vertex data, Node t)
		{
			if (t == m_Bottom)
			{
				return false;
			}
			else if (t.data.Equals(data))
			{
				return true;
			}
			else if (data < t.data)
			{
				return Contains(data, t.left);
			}
			else
			{
				return Contains(data, t.right);
			}
		}
	}
}