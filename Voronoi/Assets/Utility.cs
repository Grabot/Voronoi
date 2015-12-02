using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

public class Utility {
    public static float Distance(Vertex v1, Vertex v2)
    {
        return Convert.ToSingle(Math.Sqrt((v2.x - v1.x) * (v2.x - v1.x) + (v2.y - v1.y) * (v2.y - v1.y)));
    }

    public static Vertex Midpoint(Vertex v1, Vertex v2)
    {
        float mx = (v1.x / 2) + (v2.x / 2);
        float my = (v1.y / 2) + (v2.y / 2);
        return new Vertex(mx, my);
    }

    public static float Slope(Vertex v1, Vertex v2)
    {
        float value = (v2.y - v1.y) / (v2.x - v1.x);
        return value;
    }

}
