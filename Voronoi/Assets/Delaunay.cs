using System;

namespace Voronoi
{
    public sealed class Delaunay : Triangulation
    {
        public override bool AddVertex(Vertex a_Vertex)
        {
            Triangle face = FindFace(a_Vertex) as Triangle;

			if (face == null)
			{ return false; }

            AddVertex(face, a_Vertex);

            // Find halfedges of triangle
            HalfEdge h1 = face.HalfEdge;
            HalfEdge h2 = face.HalfEdge.Next.Twin.Next;
            HalfEdge h3 = face.HalfEdge.Next.Twin.Next.Next.Twin.Next;

            // Flip if needed
			LegalizeEdge(a_Vertex, h1, (Triangle)h1.Face);
			LegalizeEdge(a_Vertex, h2, (Triangle)h2.Face);
			LegalizeEdge(a_Vertex, h3, (Triangle)h3.Face);

            return true;
        }

		private void LegalizeEdge(Vertex a_Vertex, HalfEdge a_HalfEdge, Triangle a_Triangle)
        {
            // Points to test
            Vertex v1 = a_HalfEdge.Twin.Next.Next.Origin;
            Vertex v2 = a_HalfEdge.Next.Twin.Next.Next.Origin;
            Vertex v3 = a_HalfEdge.Next.Next.Twin.Next.Next.Origin;
            
            if (a_Triangle.InsideCircumcenter(v1) || a_Triangle.InsideCircumcenter(v2) || a_Triangle.InsideCircumcenter(v3))
            {
                HalfEdge h1 = a_HalfEdge.Twin.Next.Twin;
                HalfEdge h2 = a_HalfEdge.Twin.Prev.Twin;

                Flip(a_HalfEdge);

				LegalizeEdge(a_Vertex, h1.Twin, (Triangle)h1.Twin.Face);
				LegalizeEdge(a_Vertex, h2.Twin, (Triangle)h2.Twin.Face);
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

			if (h1.Face == null || h4.Face == null)
			{ return; }
            
            // Remove old faces
            m_Faces.Remove(a_HalfEdge.Face);
            m_Faces.Remove(a_HalfEdge.Twin.Face);

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

            m_Faces.Add(new Triangle(h1));
            m_Faces.Add(new Triangle(h1.Twin));
        }
    }
}
