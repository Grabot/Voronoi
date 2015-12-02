using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public class Triangle
{

    public Color m_colour;
    public Triangle(HalfEdge halfEdge, Color a_colour )
    {
        m_colour = a_colour;
        this.HalfEdge = halfEdge;
    }

    public HalfEdge HalfEdge;

    public bool inside(Vertex a_u1)
    {
        int i = 0;
        int j = 0;
        bool inside = false;

        List<HalfEdge> halfEdges = new List<HalfEdge>() { HalfEdge, HalfEdge.Next, HalfEdge.Next.Next };

        for (i = 0, j = 2; i < 3; j = i++)
        {
            if (((halfEdges[i].Origin.y > a_u1.y) != (halfEdges[j].Origin.y > a_u1.y)) &&
             (a_u1.x < (halfEdges[j].Origin.x - halfEdges[i].Origin.x) * (a_u1.y - halfEdges[i].Origin.y) / (halfEdges[j].Origin.y - halfEdges[i].Origin.y) + halfEdges[i].Origin.x))
                inside = !inside;
        }
        return inside;
    }

    public Vertex Circumcenter()
    {
        Vertex v1 = HalfEdge.Origin;
        Vertex v2 = HalfEdge.Next.Origin;
        Vertex v3 = HalfEdge.Next.Next.Origin;

        // Todo: Here we should add some x1-x2 > 0 logic to choose one of these, instead of trying
        Vertex c = Circumcenter(v1, v2, v3);

        if (c.isNan())
            c = Circumcenter(v2, v3, v1);

        if (c.isNan())
            c = Circumcenter(v3, v1, v2);

        return c;
    }

    private Vertex Circumcenter(Vertex a, Vertex b, Vertex c)
    {
        // determine midpoints (average of x & y coordinates)
        Vertex midAB = Midpoint(a, b);
        Vertex midBC = Midpoint(b, c);

        // determine slope
        // we need the negative reciprocal of the slope to get the slope of the perpendicular bisector
        float slopeAB = -1 / Slope(a, b);
        float slopeBC = -1 / Slope(b, c);

        // y = mx + b
        // solve for b
        float bAB = midAB.y - slopeAB * midAB.x;
        float bBC = midBC.y - slopeBC * midBC.x;

        // solve for x & y
        // x = (b1 - b2) / (m2 - m1)
        float x = (bAB - bBC) / (slopeBC - slopeAB);

        Vertex circumcenter = new Vertex(
            x,
            (slopeAB * x) + bAB
        );

        return circumcenter;
    }

    public float Diameter()
    {
        Vertex c = Circumcenter();
        Vertex v1 = HalfEdge.Origin;

        return Convert.ToSingle(Math.Sqrt((c.x - v1.x) * (c.x - v1.x) + (c.y - v1.y) * (c.y - v1.y)));
    }

    public float Slope(Vertex v1, Vertex v2)
    {
        float value = (v2.y - v1.y) / (v2.x - v1.x);
        return value;
    }

    private Vertex Midpoint(Vertex v1, Vertex v2)
    {
        float mx = (v1.x / 2) + (v2.x / 2);
        float my = (v1.y / 2) + (v2.y / 2);
        return new Vertex(mx, my);
    }
}