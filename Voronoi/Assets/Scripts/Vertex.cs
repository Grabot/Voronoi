using System;

namespace Voronoi
{
	public sealed class Vertex
    {
		public float X { get; private set; }
		public float Y { get; private set; }
		public static Vertex NaN = new Vertex(float.NaN, float.NaN);
		public enum EOwnership { UNOWNED, PLAYER1, PLAYER2 }
		public EOwnership Ownership { get; private set; }

        public Vertex(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
			this.Ownership = EOwnership.UNOWNED;
        }

		public Vertex(float X, float Y, EOwnership a_Ownership)
		{
			this.X = X;
			this.Y = Y;
			this.Ownership = a_Ownership;
		}
			
        public bool IsInvalid()
        {
			return float.IsNaN (X) || float.IsNaN (Y) || float.IsInfinity(X) || float.IsInfinity(Y);
        }

        public float DeltaSquaredXY(Vertex t)
        {
            float dx = (X - t.X);
            float dy = (Y - t.Y);
            return (dx * dx) + (dy * dy);
        }

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Vertex vertex = obj as Vertex;
			if (vertex != null)
			{ return X == vertex.X && Y == vertex.Y; }
			else
			{ return false; }
		}
    }
}
