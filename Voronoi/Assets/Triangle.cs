using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class Triangle
    {
		private bool m_Drawn = false;
		private Vertex m_Circumcenter;
		private bool m_CalculatedCircumcenter = false;
		private float circumcenterRangeSquared;
		private bool calculatedCircumcenterRangeSquared = false;
		private Vertex[] m_Vertices;
		private Color m_Color;
		private HalfEdge m_HalfEdge;

		public Vertex[] Vertices { get { return m_Vertices; } }
		public HalfEdge HalfEdge { get { return m_HalfEdge; } }
		public Color Color { get { return m_Color; } }

		public bool Drawn
		{
			get { return m_Drawn; }
			set { m_Drawn = value; }
		}

		public Triangle(HalfEdge a_HalfEdge)
		{
			m_Vertices = new Vertex[3];
			m_HalfEdge = a_HalfEdge;
			m_Color = Color.red;

			Vertex v1 = m_HalfEdge.Origin;
			Vertex v2 = m_HalfEdge.Next.Origin;
			Vertex v3 = m_HalfEdge.Next.Next.Origin;
			Vertex v4 = m_HalfEdge.Next.Next.Next.Origin;

			if (v1 == v2 || v2 == v3 || v1 == v3)
			{ Debug.LogError("Triangle does not have a correct 3 vertex loop."); }

			if (v1 != v4)
			{ Debug.LogError("Triangle does not have a correct 3 vertex loop."); }

			// Fix halfedges to this triangle
			m_HalfEdge.Triangle = this;
			m_HalfEdge.Next.Triangle = this;
			m_HalfEdge.Next.Next.Triangle = this;

			// Add vertices to the array
			m_Vertices[0] = v1;
			m_Vertices[1] = v2;
			m_Vertices[2] = v3;
		}

		public bool Inside (Vertex a_Vertex)
		{
			int i, j = m_Vertices.Length - 1;
			bool oddNodes = false;

			for (i = 0; i < m_Vertices.Length; i++)
			{
				if ((m_Vertices[i].Y < a_Vertex.Y &&
					m_Vertices[j].Y >= a_Vertex.Y ||
					m_Vertices[j].Y < a_Vertex.Y &&
					m_Vertices[i].Y >= a_Vertex.Y)
					&& (m_Vertices[i].X <= a_Vertex.X || m_Vertices[j].X <= a_Vertex.X))
				{
					oddNodes ^= (m_Vertices[i].X +
						(a_Vertex.Y - m_Vertices[i].Y) /
						(m_Vertices[j].Y - m_Vertices[i].Y) *
						(m_Vertices[j].X - m_Vertices[i].X)
					) < a_Vertex.X;
				}
				j = i;
			}

			return oddNodes;
		}

        private Vertex CalculateCircumcenter()
        {
			if (m_Vertices [0].IsInvalid () || m_Vertices [1].IsInvalid () || m_Vertices [2].IsInvalid ())
			{
				//Debug.LogError("Couldn't compute circumcircle due to invalid vertices!");
				return Vertex.NaN;
			}

			Vertex c = CalculateCircumcenterT(m_Vertices[0], m_Vertices[1], m_Vertices[2]);
			if (c.IsInvalid())
			{
				c = CalculateCircumcenterT(m_Vertices[1], m_Vertices[2], m_Vertices[0]);
				if (c.IsInvalid())
				{
					c = CalculateCircumcenterT(m_Vertices[2], m_Vertices[0], m_Vertices[1]);
					if (c.IsInvalid())
					{
						// We probably have colinear points, so solve in a non-mathematically-correct way:
						//Debug.LogWarning("Encountered colinear points while computing circumcenter!");
						float distAB = Utility.SquaredDistance (m_Vertices [0], m_Vertices [1]);
						float distBC = Utility.SquaredDistance (m_Vertices [1], m_Vertices [2]);
						float distCA = Utility.SquaredDistance (m_Vertices [2], m_Vertices [0]);

						if (distAB > distBC)
						{
							if (distAB > distCA)
							{
								c = Utility.Midpoint (m_Vertices [0], m_Vertices [1]);
							}
						}
						else
						{
							if (distBC > distCA)
							{
								c = Utility.Midpoint (m_Vertices [1], m_Vertices [2]);
							}
						}
						if (c.IsInvalid())
						{
							c = Utility.Midpoint (m_Vertices [2], m_Vertices [0]);
						}
					}
				}
			}
			if (c.IsInvalid())
			{
				//Debug.LogError("Couldn't compute circumcircle!");
				return Vertex.NaN;
			}
			return c;
		}

		private Vertex CalculateCircumcenterT(Vertex a_VertexA, Vertex a_VertexB, Vertex a_VertexC)
        {
            // determine midpoints (average of x & y coordinates)
            Vertex midAB = Utility.Midpoint(a_VertexA, a_VertexB);
            Vertex midBC = Utility.Midpoint(a_VertexB, a_VertexC);

            // determine slope
            // we need the negative reciprocal of the slope to get the slope of the perpendicular bisector
            float slopeAB = -1 / Utility.Slope(a_VertexA, a_VertexB);
            float slopeBC = -1 / Utility.Slope(a_VertexB, a_VertexC);

			// y = mx + b
			// solve for b
			float bAB = midAB.Y - slopeAB * midAB.X;
			float bBC = midBC.Y - slopeBC * midBC.X;

			// solve for x & y
			// x = (b1 - b2) / (m2 - m1)
			float x = (bAB - bBC) / (slopeBC - slopeAB);

			return new Vertex (x, (slopeAB * x) + bAB);
        }

        public bool InsideCircumcenter(Vertex a_Vertex)
        {
			return (Circumcenter.DeltaSquaredXY (a_Vertex) < CircumcenterRangeSquared);
        }
			
        public Vertex Circumcenter
        {
            get
            {
				if (m_CalculatedCircumcenter)
				{ return m_Circumcenter; }

                m_Circumcenter = CalculateCircumcenter();
				m_CalculatedCircumcenter = true;

                return m_Circumcenter;
            }
        }
			
        public float CircumcenterRangeSquared
        {
            get
            {
				if (calculatedCircumcenterRangeSquared)
				{ return circumcenterRangeSquared; }

                circumcenterRangeSquared = m_Vertices[0].DeltaSquaredXY(Circumcenter);
                calculatedCircumcenterRangeSquared = true;

                return circumcenterRangeSquared;
            }
        }
    }
}