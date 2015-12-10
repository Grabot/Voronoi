using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Voronoi
{
    public class Face
    {
        protected List<Vertex> vertices = new List<Vertex>();

        public Face(HalfEdge halfEdge)
        {
            this.HalfEdge = halfEdge;
            this.Color = Color.red;
        }

        public List<Vertex> Vertices
        {
            get { return vertices; }
        }

        public HalfEdge HalfEdge { get; private set; }

        public Color Color;

        public bool Contains(Vertex vertex)
        {
            return vertices.Contains(vertex);
        }

        public bool inside (Vertex p)
        {
            int i, j = vertices.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < vertices.Count; i++)
            {
                if ((vertices[i].Y < p.Y && vertices[j].Y >= p.Y
                || vertices[j].Y < p.Y && vertices[i].Y >= p.Y)
                && (vertices[i].X <= p.X || vertices[j].X <= p.X))
                {
                    oddNodes ^= (vertices[i].X + (p.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) * (vertices[j].X - vertices[i].X) < p.X);
                }
                j = i;
            }

            return oddNodes;
        }        
    }
}
