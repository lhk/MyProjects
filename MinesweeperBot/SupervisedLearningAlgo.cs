using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using System.Threading.Tasks;
using System.Threading;


namespace MinesweeperBot
{
    public partial class SupervisedLearningAlgo : Form
    {
        //Matrix<double> parameterMatrix;


        public SupervisedLearningAlgo()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
        }

        public static Vector<double> forwardPropStep(Matrix<double> parameterMatrix, Vector<double> input)
        {
            var extendedInput = new DenseVector(input.Count+1, 1);
            extendedInput.SetSubVector(1, input.Count, input);
            // weighted summation
            var z = parameterMatrix * extendedInput;
            // sigmoid activation
            for (int i = 0; i < z.Count; i++) z[i] = 1 / (1 + Math.Exp(-z[i]));
            return z;
        }
        
        private static Vector<double> forwardProp(double[] NeuronalNetworkParameters, Vector<double> input)
        {
            input =  normalizeIput(input);
            int parameterListIndex=0;
            for (int i = 0; i < Storage.s.NeuronalNetworkConfiguration.Length-1; i++)
            {
                Matrix<double> parameterMatrix = new DenseMatrix(Storage.s.NeuronalNetworkConfiguration[i + 1], Storage.s.NeuronalNetworkConfiguration[i] + 1);
                for (int row = 0; row < parameterMatrix.RowCount; row++)
                {
                    for (int col = 0; col < parameterMatrix.ColumnCount; col++)
                    {
                        parameterMatrix[row, col] = NeuronalNetworkParameters[parameterListIndex++];
                    }
                }

                input = forwardPropStep(parameterMatrix, input);
            }
            return input;
        }

        private static Vector<double> normalizeIput(Vector<double> input)
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

            for (int i = 0; i < input.Count; i++)
            {
                output[i] = (input[i] - mean) / (max - min);
            }
            return output;
        }

        public static double individualError(double[] NeuronalNetworkParameters, Vector<double> input, bool[] desiredOutput)
        {
            Vector<double> output = forwardProp(NeuronalNetworkParameters, input);

            double sumError = 0;
            for (int i = 0; i < output.Count; i++)
            {
                if (desiredOutput[i]) sumError += -Math.Log(output[i]);
                else sumError += -Math.Log(1 - output[i]);
            }
            return sumError / output.Count;
        }       

        public static double overallError(double[] NeuronalNetworkParameters) { return overallError(NeuronalNetworkParameters,true); }

        public static double overallError(double[] NeuronalNetworkParameters,bool useTrainingSet)
        {
            int[] usedSet = useTrainingSet ? Storage.s.TrainingSetMapping : Storage.s.TestSetMapping;
            /*
            double sumError = 0;
            for (int i = 0; i < usedSet.Length; i++)
            {
                bool[] desiredOutput = new bool[Storage.s.ClassificationSet.Length];
                for (int j = 0; j < Storage.s.ClassificationSet.Length; j++)
                    desiredOutput[j] = Storage.s.ClassificationSet[j] == Storage.s.DataSet[usedSet[i]].Label;


                sumError += individualError(NeuronalNetworkParameters, new DenseVector(Storage.s.DataSet[usedSet[i]].Features), desiredOutput);
            }*/
            double[] result = new double[usedSet.Length];
            double sumError = 0;

            // Use type parameter to make subtotal a long, not an int
            Parallel.For(0, usedSet.Length, i =>
            {
                bool[] desiredOutput = new bool[Storage.s.ClassificationSet.Length];
                for (int j = 0; j < Storage.s.ClassificationSet.Length; j++)
                    desiredOutput[j] = Storage.s.ClassificationSet[j] == Storage.s.DataSet[usedSet[i]].Label;


                result[i] = individualError(NeuronalNetworkParameters, new DenseVector(Storage.s.DataSet[usedSet[i]].Features), desiredOutput);
            });
            foreach (var item in result)
            {
                sumError += item;
            }

            return sumError / usedSet.Length;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            List<int> activeSubsetList = new List<int>();
            List<char> allowedLabels = new List<char>("01234567xf".ToCharArray());
            Dictionary<char, int> existingLabels = new Dictionary<char, int>();

            for (int i = 0; i < Storage.s.DataSet.Count; i++)
            {
                if (allowedLabels.Contains(Storage.s.DataSet[i].Label))
                {
                    activeSubsetList.Add(i);
                    if (!existingLabels.ContainsKey(Storage.s.DataSet[i].Label)) existingLabels.Add(Storage.s.DataSet[i].Label,1);
                    else existingLabels[Storage.s.DataSet[i].Label]++;
                }
            }
            activeSubsetList.ToArray();
            var randomMapping = GenuineRandomGenerator.RandomMapping(activeSubsetList.Count);

            List<int> TrainingSetMapping = new List<int>();
            List<int> TestSetMapping = new List<int>();

            for (int i = 0; i < activeSubsetList.Count; i++)
            {
                if (i < activeSubsetList.Count * 8 / 10) 
                    TrainingSetMapping.Add(activeSubsetList[randomMapping[i]]);
                else
                    TestSetMapping.Add(activeSubsetList[randomMapping[i]]);
            }
            Storage.s.TrainingSetMapping = TrainingSetMapping.ToArray();
            Storage.s.TestSetMapping = TestSetMapping.ToArray();
            Storage.s.ClassificationSet = "01234567xfj";// new string(existingLabels.Keys.ToArray());

            var oldParameters = Storage.s.NeuronalNetworkParameters;
            Storage.s.NeuronalNetworkParameters = new double[Storage.s.NeuronalNetworkParameterCount];
            Array.Copy(oldParameters, Storage.s.NeuronalNetworkParameters, Math.Min(oldParameters.Length, Storage.s.NeuronalNetworkParameters.Length));

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Storage.s.NeuronalNetworkParameters = new double[Storage.s.NeuronalNetworkParameterCount];

            for (int i = 0; i < Storage.s.NeuronalNetworkParameters.Length; i++)
            {
                Storage.s.NeuronalNetworkParameters[i] = (GenuineRandomGenerator.GetDouble() - .5) * .001;
            }
            Storage.s.learningRate = 0.001;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //chart1.Series[0].Points.Add(overallError(Storage.s.parameterMatrix));
            //chart1.Series[1].Points.Add(overallError(Storage.s.parameterMatrix,false));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double currentError = overallError(Storage.s.NeuronalNetworkParameters);

            for (int n = 0; n < numericUpDown1.Value; n++)
            {
                var NeuronalNetworkParametersPlus = new double[Storage.s.NeuronalNetworkParameters.Length];
                var NeuronalNetworkParametersMinus = new double[Storage.s.NeuronalNetworkParameters.Length];

                for (int i = 0; i < Storage.s.NeuronalNetworkParameters.Length; i++)
                {
                    var rnd = (GenuineRandomGenerator.GetDouble() - .5) * Storage.s.learningRate;
                    NeuronalNetworkParametersPlus[i] = Storage.s.NeuronalNetworkParameters[i] + rnd;
                    NeuronalNetworkParametersMinus[i] = Storage.s.NeuronalNetworkParameters[i] - rnd;
                }

                var errorPlus = overallError(NeuronalNetworkParametersPlus);
                var errorMinus = overallError(NeuronalNetworkParametersMinus);

                if (errorPlus < currentError || errorMinus < currentError)
                {
                    if (errorPlus < currentError)
                    {
                        Storage.s.NeuronalNetworkParameters = NeuronalNetworkParametersPlus;
                        currentError = errorPlus;
                    }
                    else if (errorMinus < currentError)
                    {
                        Storage.s.NeuronalNetworkParameters = NeuronalNetworkParametersMinus;
                        currentError = errorMinus;
                    }

                    Storage.s.learningRate *= 1.01;
                    chart1.Series[0].Points.Add(currentError);
                    chart1.Series[1].Points.Add(overallError(Storage.s.NeuronalNetworkParameters, false));
                }
                else Storage.s.learningRate *= 0.99;
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Training Set: " +
            (Quality(Storage.s.TrainingSetMapping) * 100).ToString("0.0") +
            "\nTest Set: " +
            (Quality(Storage.s.TestSetMapping) * 100).ToString("0.0"));
        }

        public static double Quality(int[] usedSet)
        {

            double rightCounter = 0;
            double wrongCounter = 0;

            for (int i = 0; i < usedSet.Length; i++)
            {
                if (Storage.s.DataSet[usedSet[i]].Label == evaluateFunction(Storage.s.DataSet[usedSet[i]]))
                    rightCounter++;
                else wrongCounter++;
            }

            return (rightCounter) / (rightCounter + wrongCounter);
        }

        public static char evaluateFunction(DataPoint p)
        {
            if (p == null || Storage.s.NeuronalNetworkParameters == null || Storage.s.ClassificationSet == null) return '?';
            var output = forwardProp(Storage.s.NeuronalNetworkParameters, new DenseVector(p.Features));
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
            if (highestProbability < .5) return '?';
            return Storage.s.ClassificationSet[outputLabelIndex];
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Storage.s.learningRate *= 10;
        }
    }
}
