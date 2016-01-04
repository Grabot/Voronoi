using System;
using System.Collections.Generic;

namespace Voronoi
{
    public class Vertex
    {
        public float X;
        public float Y;

        public Vertex()
        {
            X = float.NaN;
            Y = float.NaN;
        }

        public Vertex (float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vertex(Vertex v)
        {
            X = v.X;
            Y = v.Y;
        }

        public bool isNan()
        {
            return float.IsNaN(X) || float.IsNaN(Y);
        }

        public float DeltaSquaredXY(Vertex t)
        {
            float dx = (X - t.X);
            float dy = (Y - t.Y);
            return (dx * dx) + (dy * dy);
        }

        #region Operators
        public static Vertex operator -(Vertex v, Vertex w)
        {
            return new Vertex(v.X - w.X, v.Y - w.Y);
        }

        public static Vertex operator +(Vertex v, Vertex w)
        {
            return new Vertex(v.X + w.X, v.Y + w.Y);
        }

        public static float operator *(Vertex v, Vertex w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public static Vertex operator *(Vertex v, float mult)
        {
            return new Vertex(v.X * mult, v.Y * mult);
        }

        public static Vertex operator *(float mult, Vertex v)
        {
            return new Vertex(v.X * mult, v.Y * mult);
        }

        public float Cross(Vertex v)
        {
            return X * v.Y - Y * v.X;
        }
        #endregion

    }
}
