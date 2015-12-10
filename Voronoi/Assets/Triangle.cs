using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voronoi
{
    public class Triangle : Face
    {
        public Triangle(HalfEdge halfEdge) : base(halfEdge)
        {
            Vertex v1 = HalfEdge.Origin;
            Vertex v2 = HalfEdge.Next.Origin;
            Vertex v3 = HalfEdge.Next.Next.Origin;
            Vertex v4 = HalfEdge.Next.Next.Next.Origin;

            if (v1 == v2 || v2 == v3 || v1 == v3)
                throw new IncorrectTriangleException("Triangle does not has a correct 3 vertex loop.");

            if (v1 != v4)
                throw new IncorrectTriangleException("Triangle does not has a correct 3 vertex loop.");

            // Fix halfedges to this face
            halfEdge.Face = this;
            halfEdge.Next.Face = this;
            halfEdge.Next.Next.Face = this;

            // Add vertices to the array
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
        }
        
        private Vertex calculateCircumcenter()
        {
            Vertex v1 = Vertices[0];
            Vertex v2 = Vertices[1];
            Vertex v3 = Vertices[2];

            // Todo: Here we should add some x1-x2 > 0 logic to choose one of these, instead of trying
            Vertex c = calculateCircumcenterT(v1, v2, v3);

            if (c.isNan())
                c = calculateCircumcenterT(v2, v3, v1);

            if (c.isNan())
                c = calculateCircumcenterT(v3, v1, v2);

            return c;
        }

        private Vertex calculateCircumcenterT(Vertex a, Vertex b, Vertex c)
        {
            // determine midpoints (average of x & y coordinates)
            Vertex midAB = Utility.Midpoint(a, b);
            Vertex midBC = Utility.Midpoint(b, c);

            // determine slope
            // we need the negative reciprocal of the slope to get the slope of the perpendicular bisector
            float slopeAB = -1 / Utility.Slope(a, b);
            float slopeBC = -1 / Utility.Slope(b, c);

            // y = mx + b
            // solve for b
            float bAB = midAB.Y - slopeAB * midAB.X;
            float bBC = midBC.Y - slopeBC * midBC.X;

            // solve for x & y
            // x = (b1 - b2) / (m2 - m1)
            float x = (bAB - bBC) / (slopeBC - slopeAB);

            Vertex circumcenter = new Vertex(
                x,
                (slopeAB * x) + bAB
            );

            return circumcenter;
        }

        public bool InsideCircumcenter(Vertex vertex)
        {
            return (Circumcenter.DeltaSquaredXY(vertex) < CircumcenterRangeSquared);
        }

        private Vertex circumcenter;
        private bool calculatedCircumcenter = false;
        public Vertex Circumcenter
        {
            get
            {
                if (calculatedCircumcenter)
                    return circumcenter;

                circumcenter = calculateCircumcenter();
                calculatedCircumcenter = true;

                return circumcenter;
            }
        }

        private float circumcenterRangeSquared;
        private bool calculatedCircumcenterRangeSquared = false;
        public float CircumcenterRangeSquared
        {
            get
            {
                if (calculatedCircumcenterRangeSquared)
                    return circumcenterRangeSquared;

                circumcenterRangeSquared = vertices[0].DeltaSquaredXY(Circumcenter);
                calculatedCircumcenterRangeSquared = true;

                return circumcenterRangeSquared;
            }
        }
    }
}


