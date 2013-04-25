using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Scalar = System.Double;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double.Solvers.StopCriterium;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Double.Solvers.Iterative;

namespace TrussSim
{
    class Truss
    {
        public const int Dimensions = 2;
        TrussNode[] nodes;
        TrussBeam[,] beams;
        double MaxTensileStress = Scalar.MinValue;
        double MinTensileStress = Scalar.MaxValue;

        public Truss()
        {
            nodes = new TrussNode[2 * 8];
            beams = new TrussBeam[nodes.Length, nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new TrussNode();
                nodes[i].Position = new DenseVector(Dimensions, 0);
                nodes[i].Position[0] = i / 2;
                nodes[i].Position[1] = i % 2;;

            }
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < nodes.Length; j++)
                {
                    Vector<Scalar> beam_ij = nodes[j].Position - nodes[i].Position;
                    Scalar beam_ij_Length = beam_ij.Norm(2);

                    if (beam_ij_Length < 1.5)
                    {
                        beams[i, j] = new TrussBeam();
                        beams[i, j].CrosssectionArea = 1e-4;
                        beams[i, j].YoungsModulus = 210e9;
                        beams[i, j].Length = beam_ij_Length;
                    }
                }
            }

            // external loads
            nodes[nodes.Length - 2].ExternalLoad = new DenseVector(new Scalar[] { 1000, -100 });
            nodes[nodes.Length - 1].ExternalLoad = new DenseVector(new Scalar[] { -1000, -100 });

            // fixed nodes
            nodes[0].Fixed = true;
            nodes[1].Fixed = true;
        }

        public void Draw(Graphics g, HomogeneousProjection screenProjection)
        {
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < nodes.Length; j++)
                {
                    if (beams[i, j] != null)
                    {
                        Color col = ColorMap.GetColor(Math.Abs(beams[i, j].TensileStress), 0, Math.Max(Math.Abs(MaxTensileStress), Math.Abs(MinTensileStress)));
                        Pen p = new Pen(col, 3);

                        Vector<Scalar> Point_i = nodes[i].Position + nodes[i].Displacement * 1e2;
                        Vector<Scalar> Point_j = nodes[j].Position + nodes[j].Displacement * 1e2;

                        Vector<Scalar> Point_i_projected = screenProjection.Project(Point_i);
                        Vector<Scalar> Point_j_projected = screenProjection.Project(Point_j);

                        //if(Math.Abs(Point_j_projected.
                        g.DrawLine(p, Helpers.VectorToPointF(Point_i_projected), Helpers.VectorToPointF(Point_j_projected));
                    }
                }
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                float radius = 10;

                if (nodes[i].Fixed)
                {
                    Vector<Scalar> Point_i = nodes[i].Position;
                    Vector<Scalar> Point_i_projected = screenProjection.Project(Point_i);

                    g.FillEllipse(new SolidBrush(Color.Black), (float)Point_i_projected[0] - radius, (float)Point_i_projected[1] - radius, 2 * radius, 2 * radius);
                }
                else if (nodes[i].ExternalLoad != null)
                {
                    // draw force arrow

                    Scalar scale = 1e-3;
                    HomogeneousProjection arrowTipProjection = new HomogeneousProjection(2);
                    Scalar angle = Math.Atan2(nodes[i].ExternalLoad[1], nodes[i].ExternalLoad[0]);
                    arrowTipProjection.ProjectionMatrix =
                        HomogeneousProjection.Translate(nodes[i].ExternalLoad * scale + nodes[i].Position) *
                        HomogeneousProjection.Rotate(angle) *
                        HomogeneousProjection.Scale(.1,2);
                        

                    Pen p = new Pen(Color.Red, 3);
                    g.DrawLine(p, 
                        Helpers.VectorToPointF(screenProjection.Project(nodes[i].Position)),
                        Helpers.VectorToPointF(screenProjection.Project(nodes[i].Position + nodes[i].ExternalLoad * scale)));

                    g.DrawLine(p,
                        Helpers.VectorToPointF(screenProjection.Project(arrowTipProjection.Project(new DenseVector(new Scalar[] { -2, 1 })))),
                        Helpers.VectorToPointF(screenProjection.Project(arrowTipProjection.Project(new DenseVector(new Scalar[] { 0, 0 })))));

                    g.DrawLine(p,
                        Helpers.VectorToPointF(screenProjection.Project(arrowTipProjection.Project(new DenseVector(new Scalar[] { -2, -1 })))),
                        Helpers.VectorToPointF(screenProjection.Project(arrowTipProjection.Project(new DenseVector(new Scalar[] { 0, 0 })))));
                }
            }
        }

        public void Calc()
        {
            Matrix<Scalar> forceEquilibriumEquations = new SparseMatrix(nodes.Length * Dimensions);

            // iterate over all nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                // iterate over all space dimension for each node
                for (int m = 0; m < Dimensions; m++)
                {
                    // each loop itereation in here corresponds to an equation line/row in the matrix
                    // rowindex = i*Dimensions+m;

                    Vector<Scalar> MainCoefficients = new DenseVector(Dimensions, 0);

                    // iterate over all beams (i,j)
                    for (int j = 0; j < nodes.Length; j++) if (i != j && (beams[i, j] != null || beams[j, i] != null))
                    {
                        TrussBeam Beam_ij = (i < j) ? beams[i, j] : beams[j, i];
                        if (Beam_ij == null) throw new Exception("Unexpected error, algorithm failure");

                        Vector<Scalar> e = (nodes[i].Position - nodes[j].Position).Normalize(2);
                        double c = Beam_ij.YoungsModulus * Beam_ij.CrosssectionArea / Beam_ij.Length;

                        for (int n = 0; n < Dimensions; n++)
                        {
                            MainCoefficients[n] += -e[m] * c * e[n];
                            forceEquilibriumEquations[i * Dimensions + m, j * Dimensions + n] = e[m] * c * e[n];
                        }
                    }

                    for (int n = 0; n < Dimensions; n++)
                        forceEquilibriumEquations[i * Dimensions + m, i * Dimensions + n] = MainCoefficients[n];
                }
            }

            Vector<Scalar> LoadVector = new DenseVector(nodes.Length * Dimensions, 0);

            // apply loads
            for (int i = 0; i < nodes.Length; i++) if (nodes[i].ExternalLoad != null)
                {
                    for (int m = 0; m < Dimensions; m++)
                    {
                        LoadVector[i * Dimensions + m] = -nodes[i].ExternalLoad[m];
                    }
                }

            // apply bearings
            for (int i = 0; i < nodes.Length; i++) if (nodes[i].Fixed)
                {
                    for (int m = 0; m < Dimensions; m++)
                    {
                        for (int col = 0; col < nodes.Length * Dimensions; col++)
                        {
                            forceEquilibriumEquations[i * Dimensions + m, col] = 0;
                        }
                        forceEquilibriumEquations[i * Dimensions + m, i * Dimensions + m] = 1;
                        LoadVector[i * Dimensions + m] = 0;
                    }
                }

            // solve eqn system
            Vector<Scalar> DisplacementsSolutionTmp = Solve(forceEquilibriumEquations, LoadVector);

            // convert solution vector in to node-wise displacement vectors
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Displacement = new DenseVector(Dimensions, 0);
                for (int m = 0; m < Dimensions; m++)
                    nodes[i].Displacement[m] = DisplacementsSolutionTmp[i * Dimensions + m];
            }


            // compute beam tension
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                for (int j = i + 1; j < nodes.Length; j++)
                {
                    if (beams[i, j] != null)
                    {
                        beams[i, j].LengthLoaded = ((nodes[i].Position + nodes[i].Displacement) - (nodes[j].Position + nodes[j].Displacement)).Norm(2);

                        MaxTensileStress = Math.Max(beams[i, j].TensileStress, MaxTensileStress);
                        MinTensileStress = Math.Min(beams[i, j].TensileStress, MinTensileStress);
                    }
                }
            }
        }

        Vector<Scalar> Solve(Matrix<Scalar> m, Vector<Scalar> v)
        {
            for (int i = 0; i < v.Count; i++)
            {
                v[i] += 1e-10;
                for (int j = 0; j < m.ColumnCount; j++) m[i, j] += 1e-10;
            }
            var iterationCountStopCriterium = new IterationCountStopCriterium(100);

            // Stop calculation if residuals are below 1E-10 --> the calculation is considered converged
            var residualStopCriterium = new ResidualStopCriterium(1e-6);

            // Create monitor with defined stop criteriums
            var monitor = new Iterator(new IIterationStopCriterium[] { iterationCountStopCriterium, residualStopCriterium });

            // Create Bi-Conjugate Gradient Stabilized solver
            var solver = new TFQMR(monitor);
            Vector<Scalar> res = solver.Solve((Matrix)m, (Vector)v);
            return res;
        }
    }
}
