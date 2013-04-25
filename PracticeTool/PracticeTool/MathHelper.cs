using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers.StopCriterium;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Double.Solvers.Iterative;

namespace PracticeTool
{
    public static class MathHelper
    {
        /// <summary>
        /// Calculates the intersection point of two 2D rays
        /// </summary>
        /// <param name="p11">Point on first ray</param>
        /// <param name="p12">Point on first ray</param>
        /// <param name="p21">Point on second ray</param>
        /// <param name="p22">Point on second ray</param>
        /// <returns></returns>
        public static void RayIntersection(Vector<double> p11, Vector<double> p12, Vector<double> p21, Vector<double> p22, ref Vector<double> result)
        {
            Vector<double> d1 = p12.Subtract(p11);
            Vector<double> d2 = p22.Subtract(p21);
            Vector<double> d3 = p21.Subtract(p11);

            Matrix<double> m = Matrix.CreateFromColumns(new Vector<double>[] { d1, d2 });
           

                Vector<double> ab = Solve(m, d3);

                Vector<double> intersectionPoint = p11.Add(d1.Multiply(ab[0]));
                result[0] = intersectionPoint[0];
                result[1] = intersectionPoint[1];
        }

        public static Vector<double> Solve(Matrix<double> m, Vector<double> v)
        {
            for (int i = 0; i < v.Count; i++)
            {
                v[i] += 1e-10;
                for (int j = 0; j < m.ColumnCount; j++)  m[i,j] += 1e-10;
            }
            var iterationCountStopCriterium = new IterationCountStopCriterium(100);

            // Stop calculation if residuals are below 1E-10 --> the calculation is considered converged
            var residualStopCriterium = new ResidualStopCriterium(1e-6f);

            // Create monitor with defined stop criteriums
            var monitor = new Iterator(new IIterationStopCriterium[] { iterationCountStopCriterium, residualStopCriterium });

            // Create Bi-Conjugate Gradient Stabilized solver
            var solver = new TFQMR(monitor);
            Vector<double> res = solver.Solve((Matrix)m, (Vector)v);
            return res;
        }

        public static void PointRayPerpendicularPoint(Vector<double> r1, Vector<double> r2, Vector<double> p, ref Vector<double> result)
        {
            Vector<double> p2 = r1.Subtract(r2);
            p2[0] = p2[1] + p[0];
            p2[1] = -p2[0] + p[1];
            Vector<double> x = new DenseVector(2);
            RayIntersection(r1, r2, p, p2, ref result);
        }

        public static Vector<double> Get2DVector(double x, double y)
        {
            return new DenseVector(new double[] { x, y });
        }
    }
}