using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MinesweeperBot
{
    public class PrincipalComponentAnalysis : DBObject
    {
        const int digits = 6;
        double[] Mean, Std;
        Matrix<double> U_reduce_transpose;

        Dictionary<string, Vector<double>> cache = new Dictionary<string, Vector<double>>();

        public PrincipalComponentAnalysis()
        {
            Type = "PrincipalComponentAnalysis";
        }

        public override string Serialize()
        {
            StringBuilder s = new StringBuilder();
            s.Append(U_reduce_transpose.RowCount.ToString() + ":" + U_reduce_transpose.ColumnCount.ToString() + "\n");
            s.Append(FormatHelper.DoubleArrayToString(U_reduce_transpose.ToRowWiseArray(), digits));
            s.Append('\n');
            s.Append(FormatHelper.DoubleArrayToString(Mean, digits));
            s.Append('\n');
            s.Append(FormatHelper.DoubleArrayToString(Std, digits));
            return s.ToString();
        }

        public override void Deserialize(string s)
        {
            var s2 = s.Split('\n');
            var rows_cols = s2[0].Split(':');
            int rows, cols;
            if (s2.Length >= 4 && rows_cols.Length >= 2 && int.TryParse(rows_cols[0], out rows) && int.TryParse(rows_cols[1], out cols))
            {
                var U_reduce_transpose_sequential = FormatHelper.StringToDoubleArray(s2[1]);
                U_reduce_transpose = new DenseMatrix(rows,cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        U_reduce_transpose[i, j] = U_reduce_transpose_sequential[i * cols + j];

                Mean = FormatHelper.StringToDoubleArray(s2[2]);
                Std = FormatHelper.StringToDoubleArray(s2[3]);
            }
            else throw new Exception("Invalid DB Data");

        }

        public void PCASetup()
        {
            // step 1: data normalization
            Mean = new double[Storage.DataPoints[0].Features.Length];
            Std = new double[Storage.DataPoints[0].Features.Length];
            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                for (int j = 0; j < Mean.Length; j++)
                {
                    Mean[j] += Storage.DataPoints[i].Features[j];
                }
            }
            for (int j = 0; j < Mean.Length; j++)
            {
                Mean[j] /= Storage.DataPoints.Count;
            }

            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                for (int j = 0; j < Std.Length; j++)
                {
                    Std[j] += (Storage.DataPoints[i].Features[j] - Mean[j]) * (Storage.DataPoints[i].Features[j] - Mean[j]);
                }
            }
            for (int j = 0; j < Mean.Length; j++)
            {
                Std[j] = Math.Sqrt(Std[j] / Storage.DataPoints.Count);
            }

            
            // step 2: covariance Matrix
            Matrix<double> B = new DenseMatrix(Storage.DataPoints[0].Features.Length, Storage.DataPoints.Count);

            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                B.SetColumn(i,Normalize(Storage.DataPoints[i].Features));
            }
            Matrix<double> covarianceMatrix = (B * B.Transpose()) * (1.0 / Storage.DataPoints.Count);

            // step 3: svd
            var svd_Solver = new MathNet.Numerics.LinearAlgebra.Double.Factorization.DenseSvd((DenseMatrix)covarianceMatrix, true);
            var S = svd_Solver.S();

            // step 4: find output space dimensions
            double sumSingularValues = 0;
            for (int i = 0; i < S.Count; i++)
                sumSingularValues += S[i];

            int k = 0;
            double sumSingularValues2 = 0;
            for (; k < S.Count; k++)
            {
                sumSingularValues2 += S[k];
                if (sumSingularValues2 / sumSingularValues > .9999) break;
            }

            U_reduce_transpose = svd_Solver.U().SubMatrix(0, Storage.DataPoints[0].Features.Length, 0, k).Transpose();
            //var costRight = OverallCost(svd_Solver.U().SubMatrix(0, Storage.DataPoints[0].Features.Length, 0, k).Transpose());
            //var costWrong = OverallCost(svd_Solver.U().SubMatrix(0, k, 0, Storage.DataPoints[0].Features.Length));
            //var costPerfect = OverallCost(svd_Solver.U());
        }

        private Vector<double> Normalize(double[] p)
        {
            Vector<double> v = new DenseVector(p.Length);
            for (int i = 0; i < p.Length; i++)
            {
                v[i] = (p[i] - Mean[i]) / (Std[i] + .1);
            }
            return v;
        }

        /*double OverallCost(Matrix<double> m)
        {
            m = m.Transpose() * m;
            double cost = 0;
            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                var v = new DenseVector(Storage.DataPoints[i].Features);
                var diff = v - (m * v);
                cost += diff.DotProduct(diff);
            }
            return cost / Storage.DataPoints.Count;
        }

        public Vector<double> EvaluateFunction(Vector<double> input)
        {
            return U_reduce_transpose * input;
        }

        public Vector<double> Reconstruct(Vector<double> input)
        {
            return U_reduce_transpose.Transpose() * input;
        }*/

        internal Vector<double> Reduce(string id, Vector<double> input)
        {
            if (id == null) return U_reduce_transpose * input;

            lock (cache)
            {
                if (!cache.ContainsKey(id))
                    cache[id] = U_reduce_transpose * input;

                return cache[id];
            }
        }
    }
}
