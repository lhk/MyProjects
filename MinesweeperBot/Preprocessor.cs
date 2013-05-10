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
        public Vector<double> Mean;
        public Vector<double> Std;

        public Preprocessor()
        {
            //SetupFeatureRegularization();
        }

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

        public void SetupFeatureRegularization()
        {
            Mean = new DenseVector(Storage.DataPoints[0].Features.Length,0);
            Std = new DenseVector(Storage.DataPoints[0].Features.Length,0);

            for (int i = 0; i < Storage.DataPoints.Count; i++)
                for (int j = 0; j < Mean.Count; j++)
                    Mean[j] += Storage.DataPoints[i].Features[j];

            for (int j = 0; j < Mean.Count; j++)
                Mean[j] /= Storage.DataPoints.Count;

            for (int i = 0; i < Storage.DataPoints.Count; i++)
                for (int j = 0; j < Mean.Count; j++)
                    Std[j] += (Storage.DataPoints[i].Features[j] - Mean[j]) * (Storage.DataPoints[i].Features[j] - Mean[j]);

            for (int j = 0; j < Mean.Count; j++)
                Std[j] = Math.Sqrt(Std[j] / (Storage.DataPoints.Count - 1));
        }
    }
}
