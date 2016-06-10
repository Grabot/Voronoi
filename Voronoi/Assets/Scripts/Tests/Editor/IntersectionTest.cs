using UnityEngine;
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
			}
			StatusTree status = new StatusTree();
			HashSet<Edge> newEdges = new HashSet<Edge>();
			HashSet<Edge> exitingEdges = new HashSet<Edge>();
			int count = 0;
			while (eventQueue.Size != 0)
			{
				count++;
				Debug.Log("iteration " + count);
				Debug.Log ("Size: " + eventQueue.Size + " Computed: " + eventQueue.ComputeSize());
				Assert.IsTrue(eventQueue.Size == eventQueue.ComputeSize());
				Debug.Log("Event queue size: " + eventQueue.Size);
				Vertex max = eventQueue.FindMax();
				Assert.IsTrue(eventQueue.Delete(max));
				foreach (HalfEdge h in max.IncidentEdges)
				{
					if (h.ParentEdge.UpperEndpoint == max)
					{
						newEdges.Add(h.ParentEdge);
					}
					else if (h.ParentEdge.LowerEndpoint == max)
					{
						exitingEdges.Add(h.ParentEdge);
					}
				}
				foreach (Edge edge in exitingEdges)
				{
					Assert.IsTrue(status.Delete(edge));
				}
				foreach (Edge edge in newEdges)
				{
					Assert.IsTrue(status.Insert(edge));
				}
			}
			Assert.IsTrue(status.Size == 0);
			Assert.IsTrue(status.ComputeSize() == 0);
		}
	}
}
