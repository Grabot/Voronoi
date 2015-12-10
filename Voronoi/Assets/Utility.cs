using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voronoi
{
    public class Utility
    {
        public static float Distance(Vertex v1, Vertex v2)
        {
            return Convert.ToSingle(Math.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y)));
        }

        public static Vertex Midpoint(Vertex v1, Vertex v2)
        {
            float mx = (v1.X / 2) + (v2.X / 2);
            float my = (v1.Y / 2) + (v2.Y / 2);
            return new Vertex(mx, my);
        }

        public static float Slope(Vertex v1, Vertex v2)
        {
            float value = (v2.Y - v1.Y) / (v2.X - v1.X);
            return value;
        }

    }
}
