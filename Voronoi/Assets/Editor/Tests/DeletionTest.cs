using NUnit.Framework;
using DCEL = VoronoiDCEL.DCEL<int>;
using Vertex = VoronoiDCEL.Vertex<int>;
using Edge = VoronoiDCEL.Edge<int>;
using StatusTree = VoronoiDCEL.StatusTree<int>;
using HalfEdge = VoronoiDCEL.HalfEdge<int>;
using Face = VoronoiDCEL.Face<int>;
using System.Collections.Generic;
using UnityEngine;

namespace VoronoiDCEL.Tests
{
    [TestFixture]
    [Category("DCEL Deletions")]
    public class DeletionTest
    {
        [Test]
        public void TestDelete()
        {
            DCEL dcel = new DCEL();
            dcel.AddEdge(0, 0, 0, 1);
            dcel.AddEdge(0, 1, 1, 1);
            dcel.AddEdge(1, 1, 1, 0);
            dcel.AddEdge(1, 0, 0, 0);
            dcel.Initialize();
            Assert.AreEqual(4, dcel.Vertices.Count);
            Assert.AreEqual(8, dcel.HalfEdges.Count);
            Assert.AreEqual(4, dcel.Edges.Count);
            Assert.AreEqual(1, dcel.Faces.Count);
           
            dcel.DeleteVertex(dcel.Vertices[0]);
            Assert.AreEqual(3, dcel.Vertices.Count);
            Assert.AreEqual(4, dcel.HalfEdges.Count);
            Assert.AreEqual(2, dcel.Edges.Count);
            Assert.AreEqual(0, dcel.Faces.Count);

            dcel.DeleteVertex(dcel.Vertices[0]);
            Assert.IsTrue(dcel.Vertices.Count == 2 || dcel.Vertices.Count == 0);
            Assert.IsTrue(dcel.HalfEdges.Count == 2 || dcel.HalfEdges.Count == 0);
            Assert.IsTrue(dcel.Edges.Count == 1 || dcel.Edges.Count == 0);
            Assert.AreEqual(0, dcel.Faces.Count);
        }

        [Test]
        public void GeoMathTest()
        {
            HalfEdge edge = new HalfEdge();
            edge.Origin = new Vertex(0, 0);
            edge.Twin = new HalfEdge();
            edge.Twin.Origin = new Vertex(0, 1);

            Vertex v = new Vertex(-1, 0.5d);

            Assert.IsTrue(v.LeftOfLine(edge));
            Assert.IsFalse(v.RightOfLine(edge));

            edge.Origin = new Vertex(0, 1);
            edge.Twin.Origin = new Vertex(0, 0);

            Assert.IsFalse(v.LeftOfLine(edge));
            Assert.IsTrue(v.RightOfLine(edge));

            v = new Vertex(1, 0.5d);

            Assert.IsTrue(v.LeftOfLine(edge));
            Assert.IsFalse(v.RightOfLine(edge));
        }

        [Test]
        public void TestInsideRectangle()
        {
            DCEL dcel = new DCEL();
            // inner
            dcel.AddEdge(0, 0, 0, 1);
            dcel.AddEdge(0, 1, 1, 1);
            dcel.AddEdge(1, 1, 1, 0);
            dcel.AddEdge(1, 0, 0, 0);
            // outer
            dcel.AddEdge(-2, -2, -2, 3);
            dcel.AddEdge(-2, 3, 3, 3);
            dcel.AddEdge(3, 3, 3, -2);
            dcel.AddEdge(3, -2, -2, -2);
            dcel.Initialize();
            Assert.AreEqual(8, dcel.Vertices.Count);
            Assert.AreEqual(8, dcel.Edges.Count);
            Assert.AreEqual(16, dcel.HalfEdges.Count);
            Assert.AreEqual(2, dcel.Faces.Count);
            // screen
            DCEL screen = new DCEL();
            screen.AddEdge(-1, -1, -1, 2);
            screen.AddEdge(-1, 2, 2, 2);
            screen.AddEdge(2, 2, 2, -1);
            screen.AddEdge(2, -1, -1, -1);
            screen.Initialize();
            Assert.AreEqual(4, screen.Vertices.Count);
            Assert.AreEqual(4, screen.Edges.Count);
            Assert.AreEqual(8, screen.HalfEdges.Count);
            Assert.AreEqual(1, screen.Faces.Count);
            // intersect
            DCEL overlay = new DCEL(dcel, screen);
            overlay = DCEL.MapOverlay(overlay);
            Assert.AreEqual(12, overlay.Vertices.Count);
            Assert.AreEqual(12, overlay.Edges.Count);
            Assert.AreEqual(24, overlay.HalfEdges.Count);
            Assert.AreEqual(3, overlay.Faces.Count);

            HashSet<Vertex> verticesToDelete = new HashSet<Vertex>(overlay.Vertices);
            HashSet<Vertex> verticesInsideDCEL = new HashSet<Vertex>(overlay.Vertices);
            HashSet<Vertex> verticesOutsideDCEL = new HashSet<Vertex>();
            HalfEdge start;
            HalfEdge edge;
            foreach (Face f in screen.Faces)
            {
                start = f.StartingEdge;
                edge = start;
                if (edge == null)
                {
                    break;
                }
                while (true)
                {
                    foreach (Vertex v in verticesInsideDCEL)
                    {
                        if (!v.LeftOrOnLine(edge))
                        {
                            verticesOutsideDCEL.Add(v);
                        }
                    }
                    verticesInsideDCEL.ExceptWith(verticesOutsideDCEL);
                    verticesOutsideDCEL.Clear();
                    edge = edge.Next;
                    if (edge == start || edge == null)
                    {
                        break;
                    }
                }
            }
            verticesToDelete.ExceptWith(verticesInsideDCEL);
            Assert.AreEqual(4, verticesToDelete.Count);
            Vertex deleted1 = overlay.Vertices.Find(delegate(Vertex v)
                {
                    return v.X == -2 && v.Y == -2;
                });
            Vertex deleted2 = overlay.Vertices.Find(delegate(Vertex v)
                {
                    return v.X == -2 && v.Y == 3;
                });
            Vertex deleted3 = overlay.Vertices.Find(delegate(Vertex v)
                {
                    return v.X == 3 && v.Y == -2;
                });
            Vertex deleted4 = overlay.Vertices.Find(delegate(Vertex v)
                {
                    return v.X == 3 && v.Y == 3;
                });
            Assert.IsNotNull(deleted1);
            Assert.IsNotNull(deleted2);
            Assert.IsNotNull(deleted3);
            Assert.IsNotNull(deleted4);
            Assert.IsTrue(verticesToDelete.Contains(deleted1));
            Assert.IsTrue(verticesToDelete.Contains(deleted2));
            Assert.IsTrue(verticesToDelete.Contains(deleted3));
            Assert.IsTrue(verticesToDelete.Contains(deleted4));
        }
    }
}
