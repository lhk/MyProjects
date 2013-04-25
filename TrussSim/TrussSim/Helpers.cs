using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Scalar = System.Double;
using System.Drawing;

namespace TrussSim
{
    public class Helpers
    {
        public static PointF VectorToPointF(Vector<Scalar> v)
        {
            return new PointF((float)v[0], (float)v[1]);
        }

        public static SizeF VectorToSizeF(Vector<Scalar> v)
        {
            return new SizeF((float)v[0], (float)v[1]);
        }
    }
}
