﻿using System;
using System.Collections.Generic;
using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace VoronoiDCEL
{
	public sealed class Vertex : IComparable<Vertex>, IEquatable<Vertex>
	{
		private double m_x;
		private double m_y;
		private List<HalfEdge> m_IncidentEdges; // IncidentEdges must have this vertex as origin.

		public Vertex(double a_x, double a_y)
		{
			m_x = a_x;
			m_y = a_y;
			m_IncidentEdges = new List<HalfEdge>();
		}

		public List<HalfEdge> IncidentEdges
		{
			get { return m_IncidentEdges; }
		}

		public double X
		{ get { return m_x; } }

		public double Y
		{ get { return m_y; } }

		public static bool operator <(Vertex a, Vertex b)
		{
			return a.Y < b.Y || (a.Y == b.Y && a.X < b.X);
		}

		public static bool operator >(Vertex a, Vertex b)
		{
			return a.Y > b.Y || (a.Y == b.Y && a.X > b.X);
		}

		public bool OnLine(Edge a_Edge)
		{
			return Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint) == 0;
		}

		public bool LeftOfLine(Edge a_Edge)
		{
			return Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint) > 0;
		}

		public bool RightOfLine(Edge a_Edge)
		{
			return Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint) < 0;
		}

		public int CompareTo(Edge a_Edge)
		{
			double result = Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint);
			if (result == 0)
			{
				return 0;
			}
			else if (result < 0)
			{
				return 1;
			}
			else 
			{
				return -1;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is Vertex)
			{
				return m_y == ((Vertex)obj).Y && m_x == ((Vertex)obj).X;
			}
			else
			{
				return false;
			}
		}

		public bool Equals(Vertex a_Vertex)
		{
			if (a_Vertex != null)
			{
				return m_y == a_Vertex.Y && m_x == a_Vertex.X;
			}
			else
			{
				return false;
			}
		}

		public int CompareTo(Vertex a_Vertex)
		{
			if (this < a_Vertex)
			{
				return -1;
			}
			else if (this > a_Vertex)
			{
				return 1;
			}
			else if (m_y == a_Vertex.Y && m_x == a_Vertex.X)
			{
				return 0;
			}
			else
			{
				throw new Exception("Error comparing vertex!");
			}
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				hash = hash * 23 + m_x.GetHashCode();
				hash = hash * 23 + m_y.GetHashCode();
				return hash;
			}
		}

		private static double Orient2D(Vertex a, Vertex b, Vertex c)
		{
			double[,] orientArray = new double[,]
			{
				{ a.X - c.X, a.Y - c.Y },
				{ b.X - c.X, b.Y - c.Y }
			};

			MNMatrix orientMatrix = MNMatrix.Build.DenseOfArray(orientArray);
			return orientMatrix.Determinant();
		}
	}
}

