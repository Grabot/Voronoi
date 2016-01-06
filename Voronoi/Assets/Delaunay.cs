using System;
using UnityEngine;

namespace Voronoi
{
    public sealed class Delaunay : Triangulation
    {
        public override bool AddVertex(Vertex a_Vertex)
        {
			Triangle triangle = FindTriangle(a_Vertex);

			if (triangle == null)
			{ return false; }

            AddVertex(triangle, a_Vertex);

            // Find halfedges of triangle
            HalfEdge h1 = triangle.HalfEdge;
            HalfEdge h2 = triangle.HalfEdge.Next.Twin.Next;
            HalfEdge h3 = triangle.HalfEdge.Next.Twin.Next.Next.Twin.Next;

            // Flip if needed
			LegalizeEdge(a_Vertex, h1, h1.Triangle);
			LegalizeEdge(a_Vertex, h2, h2.Triangle);
			LegalizeEdge(a_Vertex, h3, h3.Triangle);

            return true;
        }

		private void LegalizeEdge(Vertex a_Vertex, HalfEdge a_HalfEdge, Triangle a_Triangle)
        {
			if (a_Vertex == null || a_HalfEdge == null || a_Triangle == null)
			{
				Debug.LogError("Invalid call to LegalizeEdge");
				return;
			}

			Vertex v1, v2, v3;
			try
			{
	            // Points to test
	            v1 = a_HalfEdge.Twin.Next.Next.Origin;
	            v2 = a_HalfEdge.Next.Twin.Next.Next.Origin;
	            v3 = a_HalfEdge.Next.Next.Twin.Next.Next.Origin;
			}
			catch (NullReferenceException)
			{
				Debug.LogError("Null pointers in call to LegalizeEdge");
				return;
			}

			if (v1 == null || v2 == null || v3 == null)
			{
				Debug.LogError("Invalid halfedge in call to LegalizeEdge");
				return;
			}
            
            if (a_Triangle.InsideCircumcenter(v1) || a_Triangle.InsideCircumcenter(v2) || a_Triangle.InsideCircumcenter(v3))
            {
                HalfEdge h1 = a_HalfEdge.Twin.Next.Twin;
                HalfEdge h2 = a_HalfEdge.Twin.Prev.Twin;

                Flip(a_HalfEdge);

				LegalizeEdge(a_Vertex, h1.Twin, h1.Twin.Triangle);
				LegalizeEdge(a_Vertex, h2.Twin, h2.Twin.Triangle);
            }
        }
        
        public void Flip(HalfEdge a_HalfEdge)
        {
            HalfEdge h1 = a_HalfEdge;
            HalfEdge h2 = h1.Next;
            HalfEdge h3 = h2.Next;
            HalfEdge h4 = a_HalfEdge.Twin;
            HalfEdge h5 = h4.Next;
            HalfEdge h6 = h5.Next;

			if (h1.Triangle == null || h4.Triangle == null)
			{ return; }
            
            // Remove old triangles
            m_Triangles.Remove(a_HalfEdge.Triangle);
            m_Triangles.Remove(a_HalfEdge.Twin.Triangle);

            h1.Next = h6;
            h6.Prev = h1;
            h6.Next = h2;
            h2.Prev = h6;
            h2.Next = h1;
            h1.Prev = h2;
            h1.Origin = h3.Origin;

            h4.Next = h3;
            h3.Prev = h4;
            h3.Next = h5;
            h5.Prev = h3;
            h5.Next = h4;
            h4.Prev = h5;
            h4.Origin = h6.Origin;

            m_Triangles.Add(new Triangle(h1));
            m_Triangles.Add(new Triangle(h1.Twin));
        }
    }
}
