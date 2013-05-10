using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MinesweeperBot
{
    public class Preprocessor
    {
        public Vector<double> FeatureRegularization(Vector<double> vector)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < vector.Count; i++)
            {
                int y = i / 16;
                int x = i % 16;
                if (x >= 2 && y >= 2 && x < 14 && y < 14)
                    output.Add((vector[i] - 500.0) / 200.0);
            }

            return new DenseVector(output.ToArray());
        }
    }
}
