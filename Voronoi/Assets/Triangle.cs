using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class Triangle : Face
    {
		private bool m_Drawn = false;
		private Vertex m_Circumcenter;
		private bool m_CalculatedCircumcenter = false;
		private float circumcenterRangeSquared;
		private bool calculatedCircumcenterRangeSquared = false;
	
		public bool Drawn
		{
			get { return m_Drawn; }
			set { m_Drawn = value; }
		}

        public Triangle(HalfEdge m_HalfEdge) : base(m_HalfEdge)
        {
            Vertex v1 = m_HalfEdge.Origin;
            Vertex v2 = m_HalfEdge.Next.Origin;
            Vertex v3 = m_HalfEdge.Next.Next.Origin;
            Vertex v4 = m_HalfEdge.Next.Next.Next.Origin;

			if (v1 == v2 || v2 == v3 || v1 == v3)
			{ Debug.LogError("Triangle does not have a correct 3 vertex loop."); }

			if (v1 != v4)
			{ Debug.LogError("Triangle does not have a correct 3 vertex loop."); }

            // Fix halfedges to this face
            m_HalfEdge.Face = this;
            m_HalfEdge.Next.Face = this;
            m_HalfEdge.Next.Next.Face = this;

            // Add vertices to the array
            m_Vertices.Add(v1);
            m_Vertices.Add(v2);
            m_Vertices.Add(v3);
        }
        
        private Vertex CalculateCircumcenter()
        {
			Vertex v1 = m_Vertices[0];
			Vertex v2 = m_Vertices[1];
			Vertex v3 = m_Vertices[2];

            // Todo: Here we should add some x1-x2 > 0 logic to choose one of these, instead of trying
            Vertex c = CalculateCircumcenterT(v1, v2, v3);
			if (c.isNan())
			{
				c = CalculateCircumcenterT (v2, v3, v1);
				if (c.isNan())
				{
					c = CalculateCircumcenterT (v3, v1, v2);
					if (c.isNan())
					{ Debug.LogError("Circumcenter is NaN!"); }
				}
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

            Vertex circumcenter = new Vertex(x, (slopeAB * x) + bAB);

            return circumcenter;
        }

        public bool InsideCircumcenter(Vertex a_Vertex)
        {
            return (Circumcenter.DeltaSquaredXY(a_Vertex) < CircumcenterRangeSquared);
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


