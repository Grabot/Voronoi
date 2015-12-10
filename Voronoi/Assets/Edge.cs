using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voronoi
{
    public class Edge
    {
        public Edge(Vertex v1, Vertex v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public Vertex v1;
        public Vertex v2;

    }
}
