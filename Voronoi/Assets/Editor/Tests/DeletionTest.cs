using NUnit.Framework;
using DCEL = VoronoiDCEL.DCEL<int>;
using Vertex = VoronoiDCEL.Vertex<int>;
using Edge = VoronoiDCEL.Edge<int>;
using StatusTree = VoronoiDCEL.StatusTree<int>;
using HalfEdge = VoronoiDCEL.HalfEdge<int>;

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
            Assert.AreEqual(dcel.Vertices.Count, 4);
            Assert.AreEqual(dcel.HalfEdges.Count, 8);
            Assert.AreEqual(dcel.Edges.Count, 4);
            Assert.AreEqual(dcel.Faces.Count, 1);
           
            dcel.DeleteVertex(dcel.Vertices[0]);
            Assert.AreEqual(dcel.Vertices.Count, 3);
            Assert.AreEqual(dcel.HalfEdges.Count, 4);
            Assert.AreEqual(dcel.Edges.Count, 2);
            Assert.AreEqual(dcel.Faces.Count, 0);

            dcel.DeleteVertex(dcel.Vertices[0]);
            Assert.IsTrue(dcel.Vertices.Count == 2 || dcel.Vertices.Count == 0);
            Assert.IsTrue(dcel.HalfEdges.Count == 2 || dcel.HalfEdges.Count == 0);
            Assert.IsTrue(dcel.Edges.Count == 1 || dcel.Edges.Count == 0);
            Assert.AreEqual(dcel.Faces.Count, 0);
        }
    }
}
