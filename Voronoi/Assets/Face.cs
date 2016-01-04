using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class Face
    {
		protected List<Vertex> m_Vertices;
		protected Color m_Color;
		protected HalfEdge m_HalfEdge;

		public List<Vertex> Vertices { get { return m_Vertices; } }
		public HalfEdge HalfEdge { get { return m_HalfEdge; } }
		public Color Color { get { return m_Color; }}

        public Face(HalfEdge a_HalfEdge)
        {
			m_Vertices = new List<Vertex>();
            m_HalfEdge = a_HalfEdge;
            m_Color = Color.red;
        }

        public bool Contains(Vertex a_Vertex)
        {
            return m_Vertices.Contains(a_Vertex);
        }

        public bool Inside (Vertex a_Vertex)
        {
            int i, j = m_Vertices.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < m_Vertices.Count; i++)
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
    }
}
