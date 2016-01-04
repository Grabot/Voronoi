﻿using System;
using System.Collections.Generic;

namespace Voronoi
{
    public class HalfEdge
    {
		private Vertex m_Origin;
		public Face Face;
		public HalfEdge Twin;
		public HalfEdge Next;
		public HalfEdge Prev;

        public HalfEdge(Vertex a_Vertex)
        {
            m_Origin = new Vertex(a_Vertex.X, a_Vertex.Y);
			Face = null;
            Twin = null;
            Next = null;
            Prev = null;
        }

        public Vertex Origin
        {
            get { return m_Origin; }
            set { m_Origin = new Vertex(value.X, value.Y); }
        }
    }

}
