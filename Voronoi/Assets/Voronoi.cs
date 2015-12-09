using UnityEngine;
using System.Collections.Generic;

public class Voronoi {
    
    public Voronoi(List<HalfEdge> edges)
    {
        foreach (HalfEdge halfEdge in edges)
        {
            Triangle f1 = halfEdge.Face;
            Triangle f2 = halfEdge.Twin.Face;

            if (f1 is Triangle && f2 is Triangle)
            {
                Triangle t1 = f1 as Triangle;
                Triangle t2 = f2 as Triangle;

                Vertex v1 = t1.Circumcenter();
                Vertex v2 = t2.Circumcenter();
            }
        }
    }
}
