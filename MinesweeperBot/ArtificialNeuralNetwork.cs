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
        public Vector<double> Parameters;
        public double LearningRate;
        public int[] Configuration;
        public string OutputSet;

        public Dictionary<char, double> OutputErrorWeights;
        //public double[] GradientApprox;
        public char usedSet = '\0';
        double currentError = -1;

        public ArtificialNeuralNetwork()
        {
            this.Type = "ArtificialNeuralNetwork";
        }

        public override void Deserialize(string s)
        {
            var s2 = s.Split('\n');
            if (s2.Length >= 4)
            {
                Parameters = new DenseVector(FormatHelper.StringToDoubleArray(s2[0]));
                Configuration = FormatHelper.StringToIntArray(s2[1]);
                LearningRate = FormatHelper.StringToDoubleArray(s2[2])[0];
                OutputSet = s2[3];



                if (GetParameterCountFromConfiguration() != Parameters.Count) throw new Exception("Nerwork Configuration and Parameterlist don't match.");
                if (Configuration[Configuration.Length - 1] != OutputSet.Length) throw new Exception("Nerwork Configuration and Output Set don't match.");

                //GradientApprox = new double[Parameters.Count];
                currentError = overallError(Parameters);
                //CalcOutputErrorWeights();
            }
            else throw new Exception("Invalid DB Data");
        }

        /*private void CalcOutputErrorWeights()
        {
            OutputErrorWeights = new Dictionary<char, double>();
            Dictionary<char, int> LabelCounts = new Dictionary<char, int>();
            int totalCount = 0;
            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                if (usedSet == '\0' || Storage.DataPoints[i].Set == usedSet)
                {
                    totalCount++;
                    if (LabelCounts.ContainsKey(Storage.DataPoints[i].Label))
                        LabelCounts[Storage.DataPoints[i].Label]++;
                    else LabelCounts.Add(Storage.DataPoints[i].Label, 1);
                }
            }
            foreach (var item in LabelCounts)
            {
                OutputErrorWeights[item.Key] = 1 / ((double)item.Value);
            }
        }*/

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

        public int GetParameterCountFromConfiguration()
        {
            int ParameterCount = 0;
            for (int i = 0; i < Configuration.Length - 1; i++)
                ParameterCount += Configuration[i + 1] * (Configuration[i] + 1);
            return ParameterCount;
        }


        public void Init()
        {
            LearningRate = 1e-2;
            OutputSet = "01234567f";
            //Configuration = new[] { Storage.DataPoints[0].Features.Length, 10, OutputSet.Length };
            Configuration = new[] { 256, 11, OutputSet.Length };
            Parameters = new DenseVector(GetParameterCountFromConfiguration());
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i] = (GenuineRandomGenerator.GetDouble() - .5) * LearningRate;
            }
            //CalcOutputErrorWeights();
            //GradientApprox = new double[Parameters.Count];
            currentError = overallError(Parameters);
        }


        public  Vector<double> forwardPropStep(Matrix<double> parameterMatrix, Vector<double> input)
        {
            var extendedInput = new DenseVector(input.Count + 1, 1);
            extendedInput.SetSubVector(1, input.Count, input);
            // weighted summation
            var z = parameterMatrix * extendedInput;
            // sigmoid activation
            for (int i = 0; i < z.Count; i++)
            {
                if (double.IsNaN(z[i]))
                {

                }
                z[i] = 1 / (1 + Math.Exp(-z[i]));
            }
            return z;
        }

        private Vector<double> forwardProp(Vector<double> _Parameters, Vector<double> input)
        {
            input = DataPoint.Preprocess(input.ToArray());

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

                input = forwardPropStep(parameterMatrix, input);
            }
            return input;
        }


        public double individualError(Vector<double> _Parameters, Vector<double> input, char desiredOutputLabel)
        {
            Vector<double> output = forwardProp(_Parameters, input);

            double sumError = 0;
            for (int i = 0; i < output.Count; i++)
            {
                if (OutputSet[i] == desiredOutputLabel) sumError += -Math.Log(output[i]);
                else sumError += -Math.Log(1 - output[i]);
            }
            return sumError / output.Count;// *OutputErrorWeights[desiredOutputLabel];
        }

        public double overallError(Vector<double> _Parameters)
        {
            double[] individualErrors = new double[Storage.DataPoints.Count];
            int usedDataPointsCount = 0;
            
            Parallel.For(0, Storage.DataPoints.Count, i =>
            {
                if ((usedSet == '\0' || Storage.DataPoints[i].Set == usedSet) && OutputSet.Contains(Storage.DataPoints[i].Label))
                {
                    individualErrors[i] = individualError(_Parameters, new DenseVector(Storage.DataPoints[i].Features), Storage.DataPoints[i].Label);
                    usedDataPointsCount++;
                }
            });

            double sumError = 0;
            for (int i = 0; i < individualErrors.Length; i++)
            {
                if (double.IsNaN(individualErrors[i]))
                {

                }
                sumError += individualErrors[i];
            }


            return sumError / usedDataPointsCount;
        }

        public double Quality()
        {
            double rightCounter = 0;
            double wrongCounter = 0;

            for (int i = 0; i < Storage.DataPoints.Count; i++)
            {
                if (Storage.DataPoints[i].Label == EvaluateFunction(Storage.DataPoints[i].Features))
                    rightCounter++;
                else wrongCounter++;
            }

            return (rightCounter) / (rightCounter + wrongCounter);
        }

        public char EvaluateFunction(double[] features)
        {
            var output = forwardProp(Parameters, new DenseVector(features));

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

        public double OptimizationStep()
        {
            /*if (currentError < 0) currentError = overallError(Parameters);

            var ParametersPlus = new double[Parameters.Count];
            var ParametersMinus = new double[Parameters.Count];
            var Step = new double[Parameters.Count];

            for (int i = 0; i < Parameters.Count; i++)
            {
                var rnd = (GenuineRandomGenerator.GetDouble() - .5) * LearningRate;
                Step[i] = rnd;
                ParametersPlus[i] = Parameters[i] + rnd + GradientApprox[i];
                ParametersMinus[i] = Parameters[i] - rnd + GradientApprox[i];
            }

            var errorPlus = overallError(ParametersPlus);
            var errorMinus = overallError(ParametersMinus);

            if (errorPlus < currentError || errorMinus < currentError)
            {
                if (errorPlus < currentError)
                {
                    Parameters = ParametersPlus;
                    currentError = errorPlus;
                    for (int i = 0; i < Parameters.Count; i++)
                        GradientApprox[i] = (GradientApprox[i] * 5.0 + Step[i]) / 6.0;

                }
                else if (errorMinus < currentError)
                {
                    Parameters = ParametersMinus;
                    currentError = errorMinus;
                    for (int i = 0; i < Parameters.Count; i++)
                        GradientApprox[i] = (GradientApprox[i] * 5.0 - Step[i]) / 6.0;
                }
            }
            else for (int i = 0; i < Parameters.Count; i++)
                    GradientApprox[i] *= .6;
            
            return currentError;*/


            var gradient = ComputeGradient();
            var error = overallError(Parameters - gradient * LearningRate);
            if (error < currentError)
            {
                Parameters -= gradient * LearningRate;
                currentError = error;
                LearningRate *= 1.1;
            }
            else LearningRate /= 2;

            return currentError;
        }

        // backprop algorithm
        public Vector<double> ComputeGradient()
        {
            Matrix<double>[] capitalTheta = new Matrix<double>[Configuration.Length - 1];
            Matrix<double>[] capitalDelta = new Matrix<double>[Configuration.Length - 1];


            // transform parameter list to weight matricies
            {
                int parameterListIndex = 0;
                for (int i = 0; i < Configuration.Length - 1; i++)
                {
                    Matrix<double> parameterMatrix = new DenseMatrix(Configuration[i + 1], Configuration[i] + 1);
                    for (int row = 0; row < parameterMatrix.RowCount; row++)
                    {
                        for (int col = 0; col < parameterMatrix.ColumnCount; col++)
                        {
                            parameterMatrix[row, col] = Parameters[parameterListIndex++];
                        }
                    }
                    capitalTheta[i] = parameterMatrix;
                }
            }

            // init capitalDelta maticies
            for (int l = 0; l < capitalDelta.Length; l++)
                capitalDelta[l] = new DenseMatrix(capitalTheta[l].RowCount, capitalTheta[l].ColumnCount, 0);


            int DataPointCount = 0;

            // accumulate errors over all labeled datapoints
            for (int i = 0; i < Storage.DataPoints.Count; i++)
                if (OutputSet.Contains(Storage.DataPoints[i].Label))
                {
                    Vector<double>[] a = new Vector<double>[Configuration.Length];
                    Vector<double>[] z = new Vector<double>[Configuration.Length];
                    Vector<double>[] delta = new Vector<double>[Configuration.Length];

                    // input
                    a[0] = DataPoint.Preprocess(Storage.DataPoints[i].Features);

                    // output
                    Vector<double> y = new DenseVector(Configuration[Configuration.Length - 1], 0);
                    for (int j = 0; j < OutputSet.Length; j++)
                        y[j] = (OutputSet[j] == Storage.DataPoints[i].Label) ? 1 : 0;

                    // forward propagation
                    for (int l = 0; l < capitalTheta.Length; l++)
                    {
                        a[l] = extendBiasTerm(a[l]);
                        z[l + 1] = capitalTheta[l] * a[l];
                        a[l + 1] = sigmoid(z[l + 1]);
                    }

                    // backward propagation
                    delta[Configuration.Length - 1] = a[Configuration.Length - 1] - y;
                    for (int l = Configuration.Length - 2; l >= 1; l--)
                    {
                        var sigmoidPrime = a[l].PointwiseMultiply((-a[l]).Add(1));
                        var capitalThetaTransposeTimesDelta = capitalTheta[l].Transpose() * delta[l + 1];
                        delta[l] = (capitalThetaTransposeTimesDelta).PointwiseMultiply(sigmoidPrime);
                    }

                    // error accumulation
                    for (int l = 0; l < Configuration.Length - 1; l++)
                    {
                        if (l < Configuration.Length - 2) delta[l + 1] = removeBiasTerm(delta[l + 1]);
                        capitalDelta[l] = capitalDelta[l] + ((delta[l + 1]).OuterProduct(a[l]));
                    }
                    DataPointCount++;
                }

            Matrix<double>[] capitalD = new Matrix<double>[Configuration.Length - 1];

            

            for (int l = 0; l < Configuration.Length - 1; l++)
            {
                capitalD[l] = capitalDelta[l] * (1.0 / DataPointCount);
            }


            // transform back
            var Gradient = new DenseVector(Parameters.Count);
            {
                int parameterListIndex = 0;
                for (int i = 0; i < Configuration.Length - 1; i++)
                {
                    for (int row = 0; row < capitalD[i].RowCount; row++)
                    {
                        for (int col = 0; col < capitalD[i].ColumnCount; col++)
                        {
                            Gradient[parameterListIndex++] = capitalD[i][row, col];
                        }
                    }
                }
            }


            return Gradient;
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

