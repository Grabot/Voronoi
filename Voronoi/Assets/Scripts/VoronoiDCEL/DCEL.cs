using System;
using System.Collections.Generic;
using UnityEngine;
using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace VoronoiDCEL
{
    public class DCEL
    {
        private List<Vertex> m_Vertices;
        private List<Edge> m_Edges;
        private readonly List<HalfEdge> m_HalfEdges;
        private List<Face> m_Faces;

        public List<Vertex> Vertices { get { return m_Vertices; } }

        public List<Edge> Edges { get { return m_Edges; } }

        public List<HalfEdge> HalfEdges { get { return m_HalfEdges; } }

        public List<Face> Faces { get { return m_Faces; } }

        public DCEL()
        {
            m_Vertices = new List<Vertex>();
            m_Edges = new List<Edge>();
            m_HalfEdges = new List<HalfEdge>();
            m_Faces = new List<Face>();
        }

        public void AddEdge(double a_x, double a_y, double b_x, double b_y)
        {
            HalfEdge h1 = new HalfEdge();
            HalfEdge h2 = new HalfEdge();
            Edge e = new Edge();
            e.Half1 = h1;
            e.Half2 = h2;
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            h1.Twin = h2;
            h2.Twin = h1;
            Vertex v1 = null;
            Vertex v2 = null;
            foreach (Vertex v in m_Vertices)
            {
                if (Math.Abs(v.X - a_x) <= float.Epsilon && Math.Abs(v.Y - a_y) <= float.Epsilon)
                {
                    v1 = v;
                }
                else if (Math.Abs(v.X - b_x) <= float.Epsilon && Math.Abs(v.Y - b_y) <= float.Epsilon)
                {
                    v2 = v;
                }
                if (v1 != null && v2 != null)
                {
                    foreach (HalfEdge h in v1.IncidentEdges)
                    {
                        if (h.Twin.Origin == v2)
                        {
                            return;
                        }
                    }
                    break;
                }
            }
            if (v1 == null)
            {
                v1 = new Vertex(a_x, a_y);
                m_Vertices.Add(v1);
            }
            if (v2 == null)
            {
                v2 = new Vertex(b_x, b_y);
                m_Vertices.Add(v2);
            }
            h1.Origin = v1;
            h2.Origin = v2;
            v1.IncidentEdges.Add(h1);
            v2.IncidentEdges.Add(h2);
            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        public void ConnectHalfEdges()
        {
            foreach (HalfEdge h in m_HalfEdges)
            {
                Vector3 edgeDirection = new Vector3((float)(h.Twin.Origin.X - h.Origin.X), (float)(h.Twin.Origin.Y - h.Origin.Y), 0);
                edgeDirection.Normalize();
                const float turnSize = 0;
                HalfEdge mostLeftTurn = null;
                foreach (HalfEdge h2 in h.Twin.Origin.IncidentEdges)
                {
                    if (h2 != h.Twin)
                    {
                        Vector3 nextEdgeDirection = new Vector3((float)(h2.Twin.Origin.X - h2.Origin.X), (float)(h2.Twin.Origin.Y - h2.Origin.Y), 0);
                        nextEdgeDirection.Normalize();
                        float turn = Vector3.Cross(edgeDirection, nextEdgeDirection).z;
                        if (turn <= 0)
                        {
                            float size = Mathf.Abs(Vector3.Dot(edgeDirection, nextEdgeDirection) - 1);
                            if (size >= turnSize)
                            {
                                mostLeftTurn = h2;
                            }
                        }
                    }
                }
                if (mostLeftTurn != null)
                {
                    h.Next = mostLeftTurn;
                    mostLeftTurn.Previous = h;
                }
            }
        }

        public void CreateFaces()
        {
            List<HalfEdge> faceEdges = new List<HalfEdge>();
            foreach (HalfEdge h in m_HalfEdges)
            {
                if (h.IncidentFace == null)
                {
                    faceEdges.Add(h);
                    HalfEdge curEdge = h.Next;
                    while (curEdge != h)
                    {
                        if (curEdge == null)
                        {
                            break;
                        }
                        else
                        {
                            faceEdges.Add(curEdge);
                        }
                        curEdge = curEdge.Next;
                    }
                    if (curEdge == h)
                    {
                        Face f = new Face();
                        f.StartingEdge = h;
                        foreach (HalfEdge newFaceEdge in faceEdges)
                        {
                            newFaceEdge.IncidentFace = f;
                        }
                        m_Faces.Add(f);
                    }
                    faceEdges.Clear();
                }
            }
        }

        public void AddVertexOnEdge(double a_x, double a_y, Edge a_Edge)
        {
            Vertex x = new Vertex(a_x, a_y);
            HalfEdge h1 = new HalfEdge();
            HalfEdge h2 = new HalfEdge();
            Edge e1 = new Edge();
            e1.Half1 = h1;
            e1.Half2 = h2;
            h1.ParentEdge = e1;
            h2.ParentEdge = e1;

            x.IncidentEdges.Add(h1);
            h1.Origin = x;

            h1.Next = a_Edge.Half1.Next;
            h1.Previous = h2;

            h1.IncidentFace = h1.IncidentFace;

            h2.Origin = a_Edge.UpperEndpoint;
            h2.Next = h1;
            h2.Previous = a_Edge.Half1.Previous;
            h2.IncidentFace = a_Edge.Half1.IncidentFace;

            a_Edge.Half1.Previous.Next = h2;
            a_Edge.Half1.Next.Previous = h1;

            m_HalfEdges.Remove(a_Edge.Half1);

            // Now the second halfedge.
            HalfEdge h3 = new HalfEdge();
            HalfEdge h4 = new HalfEdge();
            Edge e2 = new Edge();
            e2.Half1 = h3;
            e2.Half2 = h4;
            h3.ParentEdge = e2;
            h4.ParentEdge = e2;
            HalfEdge twinEdge = a_Edge.Half2;

            h3.Origin = twinEdge.Origin;
            h3.Next = h4;
            h3.Previous = twinEdge.Previous;
            h3.IncidentFace = twinEdge.IncidentFace;

            h4.Origin = x;
            h4.Next = twinEdge.Next;
            h4.Previous = h3;
            h4.IncidentFace = twinEdge.IncidentFace;

            twinEdge.Previous.Next = h3;
            twinEdge.Next.Previous = h4;

            h1.Twin = h3;
            h3.Twin = h1;
            h2.Twin = h4;
            h4.Twin = h2;

            m_HalfEdges.Remove(twinEdge);
            m_Edges.Remove(a_Edge);

            if (a_Edge.Half1.IncidentFace.StartingEdge == a_Edge.Half1)
            {
                a_Edge.Half1.IncidentFace.StartingEdge = h1;
            }
            if (twinEdge.IncidentFace.StartingEdge == twinEdge)
            {
                twinEdge.IncidentFace.StartingEdge = h3;
            }

            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_HalfEdges.Add(h3);
            m_HalfEdges.Add(h4);
            m_Edges.Add(e1);
            m_Edges.Add(e2);
        }

        public void AddVertexInsideFace(double a_x, double a_y, HalfEdge a_h)
        {
            // Precondition: the open line segment (v, u), where u = target(h) = origin(twin(h))
            // and where v = (a_x, a_y) lies completely in f = face(h).

            Vertex v = new Vertex(a_x, a_y);
            HalfEdge h1 = new HalfEdge();
            HalfEdge h2 = new HalfEdge();
            Edge e = new Edge();
            e.Half1 = h1;
            e.Half2 = h2;
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            v.IncidentEdges.Add(h2);
            h1.Twin = h2;
            h2.Twin = h1;
            h2.Origin = v;
            h1.Origin = a_h.Twin.Origin;

            h1.IncidentFace = a_h.IncidentFace;
            h2.IncidentFace = a_h.IncidentFace;

            h1.Next = h2;
            h2.Next = a_h.Next;

            h1.Previous = a_h;
            h2.Previous = h1;

            a_h.Next = h1;
            h2.Next.Previous = h2;

            m_Vertices.Add(v);
            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        public void SplitFace(HalfEdge a_h, Vertex a_v)
        {
            // Precondition: v is incident to f = face(h) but not adjacent to 
            // u = target(h) = origin(twin(h)). And the open line segment (v, u) lies
            // completely in f.

            Vertex u = a_h.Twin.Origin;
            Face f = a_h.IncidentFace;
            Face f1 = new Face();
            Face f2 = new Face();
            HalfEdge h1 = new HalfEdge();
            HalfEdge h2 = new HalfEdge();
            Edge e = new Edge();
            e.Half1 = h1;
            e.Half2 = h2;
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            f1.StartingEdge = h1;
            f2.StartingEdge = h2;

            h1.Twin = h2;
            h2.Twin = h1;

            h2.Origin = a_v;
            h1.Origin = u;

            h2.Next = a_h.Next;
            h2.Next.Previous = h2;

            h1.Previous = a_h;
            a_h.Next = h1;

            HalfEdge i = h2;
            while (true)
            {
                i.IncidentFace = f2;
                if (i.Twin.Origin == a_v)
                {
                    break;
                }
                else
                {
                    i = i.Next;
                }
            }

            h1.Next = i.Next;
            h1.Next.Previous = h1;
            i.Next = h2;
            h2.Previous = i;
            i = h1;

            while (i.Twin.Origin != u)
            {
                i.IncidentFace = f1;
                i = i.Next;
            }

            m_Faces.Remove(f);
            m_Faces.Add(f1);
            m_Faces.Add(f2);

            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        public bool FindIntersections(out Intersection[] out_Intersections)
        {
            AATree<Vertex> eventQueue = new AATree<Vertex>();
            List<Intersection> intersections = new List<Intersection>();
            foreach (Edge e in m_Edges)
            {
                eventQueue.Insert(e.UpperEndpoint);
                eventQueue.Insert(e.LowerEndpoint);
            }
            StatusTree status = new StatusTree();
            while (eventQueue.Size != 0)
            {
                Vertex p;
                if (eventQueue.FindMax(out p))
                {
                    eventQueue.Delete(p);
                    HandleEventPoint(p, status, eventQueue, intersections);
                }
                else
                {
                    throw new Exception("Couldn't find max node in non-empty tree!");
                }
            }
            if (intersections.Count > 0)
            {
                out_Intersections = intersections.ToArray();
                return true;
            }
            out_Intersections = null;
            return false;
        }

        public sealed class Intersection
        {
            public Vertex point;
            public Edge[] upperEndpointEdges;
            public Edge[] lowerEndpointEdges;
            public Edge[] containingEdges;
        }

        private static void HandleEventPoint(Vertex a_Point, StatusTree a_Status,
                                             AATree<Vertex> a_EventQueue, List<Intersection> intersections)
        {
            HashSet<Edge> upperEndpointEdges = new HashSet<Edge>(); // U(p)
            HashSet<Edge> lowerEndpointEdges = new HashSet<Edge>(); // L(p)
            foreach (HalfEdge h in a_Point.IncidentEdges)
            {
                if (h.ParentEdge.UpperEndpoint == a_Point)
                {
                    upperEndpointEdges.Add(h.ParentEdge);
                }
                if (h.ParentEdge.LowerEndpoint == a_Point)
                {
                    lowerEndpointEdges.Add(h.ParentEdge);
                }
            }
            HashSet<Edge> containingEdges = new HashSet<Edge>(a_Status.FindNodes(a_Point));
            containingEdges.ExceptWith(upperEndpointEdges);
            containingEdges.ExceptWith(lowerEndpointEdges);
            HashSet<Edge> union = new HashSet<Edge>(lowerEndpointEdges);
            union.UnionWith(upperEndpointEdges);
            union.UnionWith(containingEdges);
            if (union.Count > 1)
            {
                Intersection intersection = new Intersection();
                intersection.point = a_Point;
                intersection.upperEndpointEdges = new Edge[upperEndpointEdges.Count];
                upperEndpointEdges.CopyTo(intersection.upperEndpointEdges);
                intersection.lowerEndpointEdges = new Edge[lowerEndpointEdges.Count];
                lowerEndpointEdges.CopyTo(intersection.lowerEndpointEdges);
                intersection.containingEdges = new Edge[containingEdges.Count];
                containingEdges.CopyTo(intersection.containingEdges);
                intersections.Add(intersection);
            }
            union = new HashSet<Edge>(lowerEndpointEdges);
            union.UnionWith(containingEdges);
            foreach (Edge e in union)
            {
                if (!a_Status.Delete(e, a_Point.Y))
                {
                    throw new Exception("Could not delete lower endpoint or containing edge from status!");
                }
            }
            union = new HashSet<Edge>(upperEndpointEdges);
            union.UnionWith(containingEdges);
            foreach (Edge e in union)
            {
                if (!a_Status.Insert(e))
                {
                    throw new Exception("Could not insert upper endpoint or containing edge into status!");
                }
            }
            if (union.Count == 0)
            {
                StatusData leftNeighbour;
                StatusData rightNeighbour;
                if (a_Status.FindLeftNeighbour(a_Point, out leftNeighbour) && a_Status.FindRightNeighbour(a_Point, out rightNeighbour))
                {
                    FindNewEvent(leftNeighbour.Edge, rightNeighbour.Edge, a_Point, a_EventQueue);
                }
            }
            else
            {
                Edge leftMost;
                if (a_Status.FindLeftMostSegmentInSet(union, out leftMost))
                {
                    Edge leftNeighbour;
                    if (a_Status.FindLeftNeighbour(leftMost, out leftNeighbour))
                    {
                        FindNewEvent(leftMost, leftNeighbour, a_Point, a_EventQueue);
                    }
                }
                else
                {
                    throw new Exception("Leftmost segment not found in status, but must exist!");
                }
                Edge rightMost;
                if (a_Status.FindRightMostSegmentInSet(union, out rightMost))
                {
                    Edge rightNeighbour;
                    if (a_Status.FindRightNeighbour(rightMost, out rightNeighbour))
                    {
                        FindNewEvent(rightMost, rightNeighbour, a_Point, a_EventQueue);
                    }
                }
                else
                {
                    throw new Exception("Rightmost segment not found in status, but must exist!");
                }
            }
        }

        public static void FindNewEvent(Edge a, Edge b, Vertex point, AATree<Vertex> eventQueue)
        {
            Vertex intersection;
            if (IntersectLines(a.UpperEndpoint, a.LowerEndpoint, b.UpperEndpoint, b.LowerEndpoint, out intersection))
            {
                if (intersection.Y < point.Y || (Math.Abs(intersection.Y - point.Y) <= double.Epsilon && intersection.X > point.X))
                {
                    if (!eventQueue.Insert(intersection))
                    {
                        Debug.Log("Did not insert event; already in queue.");
                    }
                }
            }
        }

        public static bool IntersectLines(Vertex a, Vertex b, Vertex c, Vertex d, out Vertex o_Intersection)
        {
            double numerator = Vertex.Orient2D(c, d, a);
            if ((numerator * Vertex.Orient2D(c, d, b) <= 0) && (Vertex.Orient2D(a, b, c) * Vertex.Orient2D(a, b, d) <= 0))
            {
                double[,] denominatorArray =
                    {
                        { b.X - a.X, b.Y - a.Y },
                        { d.X - c.X, d.Y - c.Y }
                    };

                MNMatrix denominatorMatrix = MNMatrix.Build.DenseOfArray(denominatorArray);
                double denominator = denominatorMatrix.Determinant();

                if (Math.Abs(denominator) <= double.Epsilon)
                { // ab and cd are parallel or equal
                    o_Intersection = Vertex.Zero;
                    return false;
                }
                else
                { // can optionally check if p is very close to b, c, or d and then flip so that a is nearest p.
                    double alpha = numerator / denominator;
                    double directionX = (b.X - a.X) * alpha;
                    double directionY = (b.Y - a.Y) * alpha;
                    o_Intersection = new Vertex(a.X + directionX, a.Y + directionY);
                    return true;
                }
            }
            else
            {
                o_Intersection = Vertex.Zero;
                return false;
            }
        }

        public void Draw()
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            foreach (HalfEdge h in m_HalfEdges)
            {
                GL.Vertex3((float)(h.Origin.X), 0, (float)(h.Origin.Y));
                GL.Vertex3((float)(h.Twin.Origin.X), 0, (float)(h.Twin.Origin.Y));
            }
            GL.End();
        }
    }
}
