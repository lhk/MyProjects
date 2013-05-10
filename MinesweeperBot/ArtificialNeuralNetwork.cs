using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class ArtificialNeuralNetwork : DBObject
    {
        const int digits = 9;
        
        public double LearningRate;
        public int[] Configuration;
        public string OutputSet;
        public Vector<double> Parameters;
        public Queue<Vector<double>> GradientHistory;
        public Vector<double> LastGradient;


        Matrix<double>[] ParametersToWeightMatricies(Vector<double> _Parameters)
        {
            // transform parameter list to weight matricies
            Matrix<double>[] res = new Matrix<double>[Configuration.Length - 1];

            int parameterListIndex = 0;
            for (int i = 0; i < Configuration.Length - 1; i++)
            {
                Matrix<double> parameterMatrix = new DenseMatrix(Configuration[i + 1], Configuration[i] + 1);
                for (int row = 0; row < parameterMatrix.RowCount; row++)
                {
                    for (int col = 0; col < parameterMatrix.ColumnCount; col++)
                    {
                        parameterMatrix[row, col] = _Parameters[parameterListIndex++];
                    }
                }
                res[i] = parameterMatrix;
            }
            return res;
        }

        Vector<double> bestParameters;
        double bestParametersScore = double.MaxValue;

        public ArtificialNeuralNetwork()
        {
            this.Type = "ArtificialNeuralNetwork";
        }

        public override string Serialize()
        {
            StringBuilder s = new StringBuilder();
            s.Append(FormatHelper.DoubleArrayToString(Parameters.ToArray(), digits));
            s.Append('\n');
            s.Append(FormatHelper.IntArrayToString(Configuration));
            s.Append('\n');
            s.Append(FormatHelper.DoubleArrayToString(new double[] { LearningRate }, digits));
            s.Append('\n');
            s.Append(OutputSet);

            return s.ToString();
        }

        public override void Deserialize(string s)
        {
            var s2 = s.Split('\n');
            if (s2.Length >= 4)
            {
                Configuration = FormatHelper.StringToIntArray(s2[1]);
                Parameters = new DenseVector(FormatHelper.StringToDoubleArray(s2[0]));
                LearningRate = FormatHelper.StringToDoubleArray(s2[2])[0];
                OutputSet = s2[3];
                bestParametersScore = double.MaxValue;

                if (GetParameterCountFromConfiguration() != Parameters.Count) throw new Exception("Nerwork Configuration and Parameterlist don't match.");
                if (Configuration[Configuration.Length - 1] != OutputSet.Length) throw new Exception("Nerwork Configuration and Output Set don't match.");
                GradientHistory = new Queue<Vector<double>>();
            }
            else throw new Exception("Invalid DB Data");
        }


        public void Init()
        {
            OutputSet = "01234567f";
            Configuration = new int[] { 144, 14, OutputSet.Length };
            LearningRate = .1;
            Parameters = new DenseVector(GetParameterCountFromConfiguration());
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i] = (GenuineRandomGenerator.GetDouble() - .5) * 10;
            }
            bestParametersScore = double.MaxValue;
            bestParameters = null;
            GradientHistory = new Queue<Vector<double>>();
        }

        public int GetParameterCountFromConfiguration()
        {
            int ParameterCount = 0;
            for (int i = 0; i < Configuration.Length - 1; i++)
                ParameterCount += Configuration[i + 1] * (Configuration[i] + 1);
            return ParameterCount;
        }

        public double Quality()
        {
            double rightCounter = 0;
            double wrongCounter = 0;
            Dictionary<char,int> wrongLabelCounter = new Dictionary<char,int>();


            for (int i = 0; i < Storage.DataPoints.Count; i++)
                if (OutputSet.Contains(Storage.DataPoints[i].Label))
                {
                    if (Storage.DataPoints[i].Label == EvaluateFunction(ParametersToWeightMatricies(Parameters), Storage.DataPoints[i].Features))
                        rightCounter++;
                    else
                    {
                        wrongCounter++;
                        if (wrongLabelCounter.ContainsKey(Storage.DataPoints[i].Label))
                            wrongLabelCounter[Storage.DataPoints[i].Label]++;
                        else
                            wrongLabelCounter.Add(Storage.DataPoints[i].Label, 1);
                    }
                }

            return (rightCounter) / (rightCounter + wrongCounter);
        }

        public char EvaluateFunction(double[] features) { return EvaluateFunction(ParametersToWeightMatricies(Parameters), features); }

        public char EvaluateFunction(Matrix<double>[] capitalTheta, double[] features)
        {
            Vector<double>[] a;

            ForwardPropagation(capitalTheta, features, out a);

            var output = removeBiasTerm(a[a.Length - 1]);

            double highestProbability = 0;
            int outputLabelIndex = -1;

            for (int j = 0; j < output.Count; j++)
            {
                if (highestProbability < output[j])
                {
                    highestProbability = output[j];
                    outputLabelIndex = j;
                }
            }
            //if (highestProbability < .3) return '?';
            return OutputSet[outputLabelIndex];
        }

        public void LBFGS_Step(object obj)
        {
            double[] x = Parameters.ToArray();
            double epsg = 0;
            double epsf = 0;
            double epsx = 0;
            int maxits = 20;
            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreate(3, x, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, maxits);
            alglib.minlbfgsoptimize(state, LBFGS_Func_Grad_Wrapper, null, obj);
            alglib.minlbfgsresults(state, out x, out rep);
            Parameters = new DenseVector(x);
        }

        void LBFGS_Func_Grad_Wrapper(double[] x, ref double func, double[] grad, object obj)
        {
            Vector<double> Gradient;
            double Error;
            ComputeGradientAndError(new DenseVector(x), out Gradient, out Error);

            func = Error;
            for (int i = 0; i < grad.Length && i < Gradient.Count; i++)
            {
                grad[i] = Gradient[i];
            }

            SupervisedLearningAlgo f = (SupervisedLearningAlgo)obj;
            f.LogError(Error);
        }

        /*public double OptimizationStep()
        {
            Vector<double> Gradient;
            double Error;
            ComputeGradientAndError(Parameters, out Gradient, out Error);

            // if current parameters are best, save them
            if (bestParametersScore > Error)
            {
                bestParametersScore = Error;
                bestParameters = Parameters.Clone();
                LearningRate *= 1.2;

                if (LastGradient != null) GradientHistory.Enqueue(LastGradient);
                while (GradientHistory.Count > 3) GradientHistory.Dequeue();
            }
            else
            {
                RevertToBestParameters();
                Error = bestParametersScore;
                LearningRate /= 3;
                GradientHistory.Dequeue();
            }

            // try improve error via gradient descent
            Vector<double> GradientHistorySum = new DenseVector(Gradient.Count);
            foreach (var g in GradientHistory) GradientHistorySum += g;
            Parameters -= (GradientHistorySum + Gradient) * LearningRate;
            LastGradient = Gradient;
         
            return Error; 
        }

        public void RevertToBestParameters()
        {
            if (bestParameters != null)
            {
                Parameters = bestParameters.Clone();
            }
            else
            {
                bestParameters = Parameters.Clone();
            }
        }*/

        // backprop algorithm
        public void ComputeGradientAndError(Vector<double> _Parameters, out Vector<double> Gradient, out double _Error)
        {
            Matrix<double>[] capitalDelta = new Matrix<double>[Configuration.Length - 1];
            var capitalTheta = ParametersToWeightMatricies(_Parameters);

            // init capitalDelta matricies
            for (int l = 0; l < capitalDelta.Length; l++)
                capitalDelta[l] = new DenseMatrix(capitalTheta[l].RowCount, capitalTheta[l].ColumnCount, 0);


            int DataPointCount = 0;
            double[] individualError = new double[Storage.DataPoints.Count];

            // accumulate errors over all labeled datapoints
            //for (int i = 0; i < Storage.DataPoints.Count; i++)
            Parallel.For(0, Storage.DataPoints.Count, i =>
                {
                    if (OutputSet.Contains(Storage.DataPoints[i].Label))
                    {
                        Vector<double>[] a;
                        Vector<double>[] delta = new Vector<double>[Configuration.Length];

                        ForwardPropagation(capitalTheta, Storage.DataPoints[i].Features, out a);

                        // output
                        Vector<double> y = new DenseVector(Configuration[Configuration.Length - 1], 0);
                        for (int j = 0; j < OutputSet.Length; j++)
                            y[j] = (OutputSet[j] == Storage.DataPoints[i].Label) ? 1 : 0;
                        y = extendBiasTerm(y);


                        // error accumulation
                        {
                            Vector<double> h = removeBiasTerm(a[a.Length - 1]);
                            Vector<double> _y = removeBiasTerm(y);
                            individualError[i] = 0;
                            for (int j = 0; j < h.Count; j++)
                            {
                                individualError[i] += -_y[j] * Math.Log(h[j]) - (1 - _y[j]) * Math.Log(1 - h[j]);
                            }
                        }

                        // backward propagation
                        delta[Configuration.Length - 1] = a[Configuration.Length - 1] - y;
                        for (int l = Configuration.Length - 2; l >= 1; l--)
                        {
                            var sigmoidPrime = a[l].PointwiseMultiply((-a[l]).Add(1));
                            var capitalThetaTransposeTimesDelta = capitalTheta[l].Transpose() * removeBiasTerm(delta[l + 1]);
                            delta[l] = (capitalThetaTransposeTimesDelta).PointwiseMultiply(sigmoidPrime);
                        }

                        // gradient accumulation
                        for (int l = 0; l < Configuration.Length - 1; l++)
                        {
                            lock (capitalDelta[l]) capitalDelta[l] = capitalDelta[l] + ((removeBiasTerm(delta[l + 1])).OuterProduct(a[l]));
                        }
                        DataPointCount++;
                    }
                }
            );

            Matrix<double>[] capitalD = new Matrix<double>[Configuration.Length - 1];

            

            for (int l = 0; l < Configuration.Length - 1; l++)
            {
                capitalD[l] = capitalDelta[l] * (1.0 / DataPointCount); // TODO regularization term
            }


            // transform back
            Gradient = new DenseVector(Parameters.Count);
            {
                int parameterListIndex = 0;
                for (int i = 0; i < Configuration.Length - 1; i++)
                    for (int row = 0; row < capitalD[i].RowCount; row++)
                        for (int col = 0; col < capitalD[i].ColumnCount; col++)
                            Gradient[parameterListIndex++] = capitalD[i][row, col];
            }

            double Error = 0;
            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                Error += individualError[i];
            }
            _Error = Error / (DataPointCount * OutputSet.Length);
        }

        private void ForwardPropagation(Matrix<double>[] capitalTheta, double[] features, out Vector<double>[] a)
        {
            a = new Vector<double>[Configuration.Length];
            Vector<double>[] z = new Vector<double>[Configuration.Length];

            // input
            a[0] = Storage.Preprocessor.FeatureRegularization(new DenseVector(features));
            a[0] = extendBiasTerm(a[0]);
             
            // forward propagation
            for (int l = 0; l < capitalTheta.Length; l++)
            {
                z[l + 1] = capitalTheta[l] * a[l];
                a[l + 1] = sigmoid(z[l + 1]);
                a[l + 1] = extendBiasTerm(a[l + 1]);
            }
        }

        private Vector<double> extendBiasTerm(Vector<double> vector)
        {
            var extendedVector = new DenseVector(vector.Count + 1, 1);
            extendedVector.SetSubVector(1, vector.Count, vector);
            return extendedVector;
        }

        private Vector<double> removeBiasTerm(Vector<double> vector)
        {
            return vector.SubVector(1, vector.Count - 1);
        }

        private Vector<double> sigmoid(Vector<double> vector)
        {
            Vector<double> res = new DenseVector(vector.Count);
            for (int i = 0; i < vector.Count; i++)
            {
                res[i] = 1 / (1 + Math.Exp(-vector[i]));
            }
            return res;
        }
    }
}

