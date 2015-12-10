using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voronoi
{
    public class IncorrectTriangleException : Exception
    {
        public IncorrectTriangleException()
        {
        }

        public IncorrectTriangleException(string message)
            : base(message)
        {
        }

        public IncorrectTriangleException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
