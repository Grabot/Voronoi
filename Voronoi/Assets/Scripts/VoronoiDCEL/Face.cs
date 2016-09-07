using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace VoronoiDCEL
{
    public sealed class Face<T>
    {
        private HalfEdge<T> m_StartingEdge;
        // arbitrary halfedge as starting point for counter-clockwise traversal.
        private T m_Data;

        public HalfEdge<T> StartingEdge
        {
            get { return m_StartingEdge; }
            set { m_StartingEdge = value; }
        }

        public T Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public Face()
        {
            m_Data = default(T);
        }

        public Face(T a_Data)
        {
            m_Data = a_Data;
        }

        public double ComputeSignedArea()
        {
            double result = 0;
            HalfEdge<T> i = m_StartingEdge;

            double[,] areaArray = new double[,]
            {
                { 0, 0 },
                { 0, 0 },
            };

            while (true)
            {
                areaArray[0, 0] = i.Origin.X;
                areaArray[0, 1] = i.Twin.Origin.X;
                areaArray[1, 0] = i.Origin.Y;
                areaArray[1, 1] = i.Twin.Origin.Y;
                MNMatrix areaMatrix = MNMatrix.Build.DenseOfArray(areaArray);
                result += areaMatrix.Determinant();
                i = i.Next;
                if (i == m_StartingEdge)
                {
                    break;
                }
            }
            return (result * 0.5d);
        }
    }
}

