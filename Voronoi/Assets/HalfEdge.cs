using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voronoi
{
    public class HalfEdge
    {
        private Vertex origin;

        public HalfEdge(Vertex v)
        {
            origin = new Vertex(v.X, v.Y);
            
            Twin = null;
            Next = null;
            Prev = null;
        }

        public Vertex Origin
        {
            get { return origin; }
            set { origin = new Vertex(value.X, value.Y); }
        }

        public Face Face;
        public HalfEdge Twin;
        public HalfEdge Next;
        public HalfEdge Prev;
    }

}
