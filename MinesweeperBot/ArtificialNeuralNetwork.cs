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
        public double[] Parameters;
        public double LearningRate;
        public int[] Configuration;
        public string OutputSet;

        public Dictionary<char, double> OutputErrorWeights;
        public double[] GradientApprox;
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
                Parameters = FormatHelper.StringToDoubleArray(s2[0]);
                Configuration = FormatHelper.StringToIntArray(s2[1]);
                LearningRate = FormatHelper.StringToDoubleArray(s2[2])[0];
                OutputSet = s2[3];



                if (GetParameterCountFromConfiguration() != Parameters.Length) throw new Exception("Nerwork Configuration and Parameterlist don't match.");
                if (Configuration[Configuration.Length - 1] != OutputSet.Length) throw new Exception("Nerwork Configuration and Output Set don't match.");

                GradientApprox = new double[Parameters.Length];
                CalcOutputErrorWeights();
            }
            else throw new Exception("Invalid DB Data");
        }

        private void CalcOutputErrorWeights()
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
        }

        public override string Serialize()
        {
            StringBuilder s = new StringBuilder();
            s.Append(FormatHelper.DoubleArrayToString(Parameters, digits));
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
            LearningRate = 1e-11;
            OutputSet = "01234567xf";
            Configuration = new[] { Storage.DataPoints[0].Features.Length, 10, OutputSet.Length };
            Parameters = new double[GetParameterCountFromConfiguration()];
            for (int i = 0; i < Parameters.Length; i++)
            {
                Parameters[i] = (GenuineRandomGenerator.GetDouble() - .5) * LearningRate;
            }
            ID = FormatHelper.hash(Serialize());
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

        private Vector<double> forwardProp(double[] _Parameters, Vector<double> input)
        {
            input = normalizeIput(input);
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

        private  Vector<double> normalizeIput(Vector<double> input)
        {
            Vector<double> output = new DenseVector(input.Count);
            double min = double.MaxValue, max = double.MinValue, mean = 0;
            for (int i = 0; i < input.Count; i++)
            {
                min = Math.Min(min, input[i]);
                max = Math.Max(max, input[i]);
                mean += input[i];
            }
            mean /= input.Count;

            if (max == min) max += 1e-8;

            for (int i = 0; i < input.Count; i++)
            {
                output[i] = (input[i] - mean) / (max - min);
            }
            return output;
        }

        public  double individualError(double[] _Parameters, Vector<double> input, char desiredOutputLabel)
        {
            Vector<double> output = forwardProp(_Parameters, input);

            double sumError = 0;
            for (int i = 0; i < output.Count; i++)
            {
                if (OutputSet[i] == desiredOutputLabel) sumError += -Math.Log(output[i]);
                else sumError += -Math.Log(1 - output[i]);
            }
            return sumError / output.Count * OutputErrorWeights[desiredOutputLabel];
        }

        public double overallError(double[] _Parameters)
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
            if (currentError < 0) currentError = overallError(Parameters);

            var ParametersPlus = new double[Parameters.Length];
            var ParametersMinus = new double[Parameters.Length];
            var Step = new double[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
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
                    for (int i = 0; i < Parameters.Length; i++)
                        GradientApprox[i] = (GradientApprox[i] * 10.0 + Step[i]) / 11.0;

                }
                else if (errorMinus < currentError)
                {
                    Parameters = ParametersMinus;
                    currentError = errorMinus;
                    for (int i = 0; i < Parameters.Length; i++)
                        GradientApprox[i] = (GradientApprox[i] * 10.0 - Step[i]) / 11.0;
                }
            }
            else for (int i = 0; i < Parameters.Length; i++)
                    GradientApprox[i] *= .95;

            return currentError;
        }
    }
}
