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
using System.Data.SQLite;


namespace MinesweeperBot
{
    public partial class SupervisedLearningAlgo : Form
    {
        double oldCost = 0;
        bool runOptimization = false;
        Thread workerThread;

        public SupervisedLearningAlgo()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            label2.Text = Storage.ANN.LearningRate.ToString("e2");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var dp in Storage.DataPoints)
                dp.Set = (GenuineRandomGenerator.GetDouble() < 0.8) ? 't' : 'e';
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (runOptimization)
            {
                // stop optimization
                runOptimization = false;
                button4.Text = "Start Optimization";
            }
            else
            {
                runOptimization = true;
                button4.Text = "Stop Optimization";
                workerThread = new Thread(new ThreadStart(RunLoop));
                workerThread.Priority = ThreadPriority.Lowest;
                workerThread.Start();
            }
            
        }

        void RunLoop()
        {
            while (runOptimization)
            {
                //double cost = Storage.ANN.OptimizationStep();

                Storage.ANN.LBFGS_Step(this);

                /*try
                {
                    Invoke((MethodInvoker)
                        delegate
                        {
                            if (oldCost != 0 && cost < 1e20 && cost > 1e-20 && oldCost < 1e20 && oldCost > 1e-20)
                            {
                                chart2.Series[0].Points.Add(oldCost - cost);
                                chart1.Series[0].Points.Add(cost);
                            }
                            oldCost = cost;
                            label2.Text = Storage.ANN.LearningRate.ToString("e2");
                        });
                }
                catch { }*/
            }
        }

        public void LogError(double cost)
        {
            try
            {
                Invoke((MethodInvoker)
                    delegate
                    {
                        if (oldCost != 0 && cost < 1e20 && cost > 1e-20 && oldCost < 1e20 && oldCost > 1e-20)
                        {
                            chart2.Series[0].Points.Add(oldCost - cost);
                            chart1.Series[0].Points.Add(cost);
                        }
                        oldCost = cost;
                        label2.Text = Storage.ANN.LearningRate.ToString("e2");
                    });
            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show((Storage.ANN.Quality() * 100).ToString("0.0"));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Storage.ANN.LearningRate *= 2;
            label2.Text = Storage.ANN.LearningRate.ToString("e2");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Storage.ANN.LearningRate /= 2;
            label2.Text = Storage.ANN.LearningRate.ToString("e2");
        }

        private void SupervisedLearningAlgo_FormClosing(object sender, FormClosingEventArgs e)
        {
            runOptimization = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really?", "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                //Storage.ANN = new ArtificialNeuralNetwork();
                Storage.ANN.Init();
            }
        }
    }
}
