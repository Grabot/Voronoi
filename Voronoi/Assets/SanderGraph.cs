using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class SanderGraph : Graph {

    private System.Random rand = new System.Random();

    public float m_Radius = 0.5f;
    public int m_sides = 50;  // The amount of segment to create the circle
    private List<Triangle> faces = new List<Triangle>();
    private List<Vertex> vertices = new List<Vertex>();
    private List<HalfEdge> halfEdges = new List<HalfEdge>();

    // When added to an object, draws colored rays from the
    // transform position.
    public int lineCount = 100;

    public Material lineMaterial;
    public void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }


    public List<Triangle> Faces { get { return faces; } }
    public List<Vertex> Vertices { get { return vertices; } }
    public List<HalfEdge> HalfEdges { get { return halfEdges; } }

    public void createGraph()
    {
        Vertex v1 = new Vertex(-5, -5);
        Vertex v2 = new Vertex(5, 5);
        Vertex v3 = new Vertex(-5, 5);
        Vertex v4 = new Vertex(5, -5);
        vertices.AddRange(new List<Vertex>() { v1, v2, v3, v4 });

        HalfEdge h1 = new HalfEdge(v1);
        HalfEdge h2 = new HalfEdge(v2);
        HalfEdge h3 = new HalfEdge(v3);
        halfEdges.AddRange(new List<HalfEdge>() { h1, h2, h3 });

        Triangle face = new Triangle(h1, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
        h1.Face = face;
        h2.Face = face;
        h3.Face = face;
        faces.Add(face);

        h1.Next = h2;
        h2.Next = h3;
        h3.Next = h1;

        h2.Prev = h1;
        h3.Prev = h2;
        h1.Prev = h3;

        HalfEdge h4 = new HalfEdge(v2);
        HalfEdge h5 = new HalfEdge(v1);
        HalfEdge h6 = new HalfEdge(v4);
        halfEdges.AddRange(new List<HalfEdge>() { h4, h5, h6 });

        Triangle face1 = new Triangle(h4, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
        h4.Face = face1;
        h5.Face = face1;
        h6.Face = face1;
        faces.Add(face1);

        h4.Twin = h1;
        h1.Twin = h4;

        h4.Next = h5;
        h5.Next = h6;
        h6.Next = h4;

        h5.Prev = h4;
        h6.Prev = h5;
        h4.Prev = h6;
    }

    // Todo: tree implementation
    public Triangle FindFace(Vertex vertex)
    {
        foreach (Triangle face in faces)
        {
            if (face.inside(vertex))
            {
                return face;
            }
        }

        return null;
    }
    
    public bool AddVertex(Vertex vertex)
    {
        Triangle face = FindFace(vertex);

        if (face == null)
            return false;

        List<Triangle> faces = AddVertex(face, vertex);
        this.faces.AddRange(faces);
        Delaunay(faces);

        return true;
    }

    private List<Triangle> AddVertex(Triangle face, Vertex vertex)
    {
        this.faces.Remove(face);

        vertices.Add(vertex);
        HalfEdge h1 = face.HalfEdge;
        HalfEdge h2 = h1.Next;
        HalfEdge h3 = h2.Next;

        HalfEdge h4 = new HalfEdge(h1.Origin);
        HalfEdge h5 = new HalfEdge(h2.Origin);
        HalfEdge h6 = new HalfEdge(h3.Origin);
        HalfEdge h7 = new HalfEdge(vertex);
        HalfEdge h8 = new HalfEdge(vertex);
        HalfEdge h9 = new HalfEdge(vertex);
        halfEdges.AddRange(new List<HalfEdge>() { h4, h5, h6, h7, h8, h9 });

        h4.Twin = h7;
        h7.Twin = h4;
        h5.Twin = h8;
        h8.Twin = h5;
        h6.Twin = h9;
        h9.Twin = h6;

        Triangle face1 = new Triangle(h1, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
        h1.Face = face1;
        h5.Face = face1;
        h7.Face = face1;

        Triangle face2 = new Triangle(h2, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
        h2.Face = face2;
        h6.Face = face2;
        h8.Face = face2;

        Triangle face3 = new Triangle(h3, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
        h3.Face = face3;
        h4.Face = face3;
        h9.Face = face3;

        List<Triangle> faces = new List<Triangle>() { face1, face2, face3 };

        // Set all next
        h1.Next = h5;
        h5.Prev = h1;
        h5.Next = h7;
        h7.Prev = h5;
        h7.Next = h1;
        h1.Prev = h7;

        h2.Next = h6;
        h6.Prev = h2;
        h6.Next = h8;
        h8.Prev = h6;
        h8.Next = h2;
        h2.Prev = h8;

        h3.Next = h4;
        h4.Prev = h3;
        h4.Next = h9;
        h9.Prev = h4;
        h9.Next = h3;
        h3.Prev = h9;

        return faces;
    }

    private void Delaunay(List<Triangle> faces)
    {
        for (int i = 0; i < faces.Count; i++)
        {
            if (!(faces[i] is Triangle))
                continue;

            Triangle face = faces[i] as Triangle;

            // Get all surrounding points
            Vertex v = face.Circumcenter();
            float d = face.Diameter();

            if (Utility.Distance(face.HalfEdge.Twin.Prev.Origin, v) < d)
            {
                Flip(face.HalfEdge);

                // Add surrounding faces
                if (face.HalfEdge.Next.Face != face)
                    faces.Add(face.HalfEdge.Next.Face);

                if (face.HalfEdge.Next.Next.Face != face)
                    faces.Add(face.HalfEdge.Next.Next.Face);

                if (face.HalfEdge.Twin.Next.Face != face)
                    faces.Add(face.HalfEdge.Twin.Next.Face);

                if (face.HalfEdge.Twin.Next.Next.Face != face)
                    faces.Add(face.HalfEdge.Twin.Next.Next.Face);
            }
        }
    }

    public void Flip(HalfEdge h)
    {
        HalfEdge h1 = h;
        HalfEdge h2 = h1.Next;
        HalfEdge h3 = h2.Next;
        HalfEdge h4 = h.Twin;
        HalfEdge h5 = h4.Next;
        HalfEdge h6 = h5.Next;

        // Set faces defined as h1 and h4
        h1.Face.HalfEdge = h1;
        h4.Face.HalfEdge = h4;

        if (h.Twin == h)
            return;

        h1.Next = h6;
        h6.Prev = h1;
        h6.Next = h2;
        h2.Prev = h6;
        h2.Next = h1;
        h1.Prev = h2;
        h1.Origin = h3.Origin;
        h6.Face = h1.Face;
        h2.Face = h1.Face;

        h4.Next = h3;
        h3.Prev = h4;
        h3.Next = h5;
        h5.Prev = h3;
        h5.Next = h4;
        h4.Prev = h5;
        h4.Origin = h6.Origin;
        h3.Face = h4.Face;
        h5.Face = h4.Face;
    }
}