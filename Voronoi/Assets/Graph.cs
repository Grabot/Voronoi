using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class Graph
    {
        protected List<Triangle> m_Triangles = new List<Triangle>();
        protected List<Vertex> m_Vertices = new List<Vertex>();
        protected List<HalfEdge> m_HalfEdges = new List<HalfEdge>();
		public Material m_LineMaterial;

		public List<Triangle> Triangles { get { return m_Triangles; } }
        public List<Vertex> Vertices { get { return m_Vertices; } }
        public List<HalfEdge> HalfEdges { get { return m_HalfEdges; } }

        public void CreateLineMaterial()
        {
			if (!m_LineMaterial) {
				// Unity has a built-in shader that is useful for drawing
				// simple colored things.
				Shader shader = Shader.Find ("Hidden/Internal-Colored");
				m_LineMaterial = new Material (shader);
				m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
				// Turn on alpha blending
				m_LineMaterial.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				m_LineMaterial.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				// Turn backface culling off
				m_LineMaterial.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
				// Turn off depth writes
				m_LineMaterial.SetInt ("_ZWrite", 0);
			}
        }

        public void Create()
        {
            Vertex v1 = new Vertex(-500000, -500000);
            Vertex v2 = new Vertex(500000, 500000);
            Vertex v3 = new Vertex(-500000, 500000);
            Vertex v4 = new Vertex(500000, -500000);
            m_Vertices.AddRange(new List<Vertex>() { v1, v2, v3, v4 });

            HalfEdge h1 = new HalfEdge(v1);
            HalfEdge h2 = new HalfEdge(v2);
            HalfEdge h3 = new HalfEdge(v3);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h1, h2, h3 });

            h1.Next = h2;
            h2.Next = h3;
            h3.Next = h1;

            h2.Prev = h1;
            h3.Prev = h2;
            h1.Prev = h3;

            HalfEdge h4 = new HalfEdge(v2);
            HalfEdge h5 = new HalfEdge(v1);
            HalfEdge h6 = new HalfEdge(v4);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h4, h5, h6 });

            h4.Twin = h1;
            h1.Twin = h4;

            h4.Next = h5;
            h5.Next = h6;
            h6.Next = h4;

            h5.Prev = h4;
            h6.Prev = h5;
            h4.Prev = h6;

            HalfEdge h7 = new HalfEdge(v1);
            HalfEdge h8 = new HalfEdge(v2);
            HalfEdge h9 = new HalfEdge(v3);
            HalfEdge h10 = new HalfEdge(v4);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h7, h8, h9, h10 });

            h10.Next = h7;
            h7.Prev = h10;
            h8.Next = h10;
            h10.Prev = h8;
            h9.Next = h8;
            h8.Prev = h9;
            h7.Next = h9;
            h9.Prev = h7;

            h3.Twin = h7;
            h7.Twin = h3;
            h8.Twin = h6;
            h6.Twin = h8;
            h2.Twin = h9;
            h9.Twin = h2;

            h10.Twin = h5;
            h5.Twin = h10;

            m_Triangles.Add(new Triangle(h1));
            m_Triangles.Add(new Triangle(h4));
        }

		protected Triangle FindTriangle(Vertex a_Vertex)
        {
			foreach (Triangle triangle in m_Triangles)
            {
                if (triangle.Inside(a_Vertex))
                {
                    return triangle;
                }
            }
            return null;
        }

    }
}
