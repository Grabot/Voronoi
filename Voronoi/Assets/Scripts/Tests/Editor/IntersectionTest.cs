using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace VoronoiDCEL.Tests
{
    [TestFixture]
    [Category("Intersections")]
    public class IntersectionTest
    {
        [Test]
        public void TestIntersections()
        {
            DCEL dcel = new DCEL();
            dcel.AddEdge(4, 4, 3, 4);
            dcel.AddEdge(5, 5, 3, 5);
            dcel.AddEdge(3, 3, 2, 4);
            AATree<Vertex> eventQueue = new AATree<Vertex>();
            int insertCount = 0;
            foreach (Vertex v in dcel.Vertices)
            {
                eventQueue.Insert(v);
                insertCount++;
                Assert.AreEqual(insertCount, eventQueue.Size);
                Assert.AreEqual(insertCount, eventQueue.ComputeSize());
                Assert.IsTrue(eventQueue.VerifyLevels());
            }
            StatusTree status = new StatusTree();
            HashSet<Edge> newEdges = new HashSet<Edge>();
            HashSet<Edge> exitingEdges = new HashSet<Edge>();
            int eventCount = 0;
            int statusCount = 0;
            while (eventQueue.Size != 0)
            {
                eventCount++;
                //Debug.Log("iteration " + eventCount);
                //Debug.Log("Event size: " + eventQueue.Size + " Computed: " + eventQueue.ComputeSize() + " Count: " + insertCount);
                Assert.IsTrue(eventQueue.Size == eventQueue.ComputeSize());
                Assert.IsTrue(eventQueue.Size == insertCount);
                Vertex max;
                Assert.IsTrue(eventQueue.FindMax(out max));
                Assert.IsNotNull(max);
                //Debug.Log("Removed vertex from event queue: " + max);
                Assert.IsTrue(eventQueue.Delete(max));
                Assert.IsTrue(eventQueue.VerifyLevels());
                insertCount--;
                foreach (HalfEdge h in max.IncidentEdges)
                {
                    if (h.ParentEdge.UpperEndpoint == max)
                    {
                        newEdges.Add(h.ParentEdge);
                        //Debug.Log("Adding vertex to status: " + max);
                    }
                    else if (h.ParentEdge.LowerEndpoint == max)
                    {
                        exitingEdges.Add(h.ParentEdge);
                        //Debug.Log("Removing vertex from status: " + max);
                    }
                    else
                    {
                        throw new Exception("The incident edge's endpoints are not equal to the max vertex!");
                    }
                }
                foreach (Edge edge in exitingEdges)
                {
                    Assert.IsTrue(status.Delete(edge));
                    Assert.IsTrue(status.VerifyLevels());
                    Assert.IsTrue(status.Size == status.ComputeSize());
                    statusCount--;
                    Assert.IsTrue(status.Size == statusCount);
                }
                exitingEdges.Clear();
                foreach (Edge edge in newEdges)
                {
                    Assert.IsTrue(status.Insert(edge));
                    Assert.IsTrue(status.VerifyLevels());
                    Assert.IsTrue(status.Size == status.ComputeSize());
                    statusCount++;
                    Assert.IsTrue(status.Size == statusCount);
                }
                newEdges.Clear();
                //Debug.Log("Status size: " + status.Size + " Computed: " + status.ComputeSize() + " Count: " + statusCount);
            }
            Assert.IsTrue(status.Size == 0);
            Assert.IsTrue(status.ComputeSize() == 0);
            Assert.IsTrue(statusCount == 0);
            Assert.IsTrue(status.VerifyLevels());
            Assert.IsTrue(eventQueue.VerifyLevels());
        }

        [Test]
        public void TestTree()
        {
            AATree<int> tree = new AATree<int>();

            Assert.IsTrue(tree.Insert(2));
            Assert.IsTrue(1 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Insert(1));
            Assert.IsTrue(2 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Delete(2));
            Assert.IsTrue(1 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Delete(1));
            Assert.IsTrue(0 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(0 == tree.Size);

            Assert.IsTrue(tree.Insert(2));
            Assert.IsTrue(1 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Insert(1));
            Assert.IsTrue(2 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Delete(1));
            Assert.IsTrue(1 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(tree.Delete(2));
            Assert.IsTrue(0 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            Assert.IsTrue(0 == tree.Size);

            int count = 0;
            for (int i = 1; i < 14; i++)
            {
                if (i != 6)
                {
                    Assert.IsTrue(tree.Insert(i));
                    count++;
                    Assert.IsTrue(count == tree.Size);
                    Assert.IsTrue(count == tree.ComputeSize());
                    Assert.IsTrue(tree.VerifyLevels());
                }
            }
            Assert.IsTrue(tree.Insert(6));
            count++;
            Assert.IsTrue(count == tree.Size);
            Assert.IsTrue(count == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            for (int i = 3; i < 14; i++)
            {
                Assert.IsTrue(tree.Delete(i));
                count--;
                Assert.IsTrue(count == tree.Size);
                Assert.IsTrue(count == tree.ComputeSize());
                Assert.IsTrue(tree.VerifyLevels());
            }
            Assert.IsTrue(tree.Insert(3));
            count++;
            Assert.IsTrue(count == tree.Size);
            Assert.IsTrue(count == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
            for (int i = 1; i < 4; i++)
            {
                Assert.IsTrue(tree.Delete(i));
                count--;
                Assert.IsTrue(count == tree.Size);
                Assert.IsTrue(count == tree.ComputeSize());
                Assert.IsTrue(tree.VerifyLevels());
            }
            Assert.IsFalse(tree.Delete(1));
            Assert.IsTrue(0 == tree.Size);
            Assert.IsTrue(0 == tree.ComputeSize());
            Assert.IsTrue(tree.VerifyLevels());
        }
    }
}
