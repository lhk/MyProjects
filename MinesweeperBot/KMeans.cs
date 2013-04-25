using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public partial class KMeans : Form
    {
        List<CentroidSet> CentroidSetHistory = new List<CentroidSet>();

        public KMeans()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;




            if (Storage.s.CentroidSet == null)
                CentroidSetHistoryAdd(RandomInit());
            else 
                CentroidSetHistoryAdd(Storage.s.CentroidSet);
        }

        private static CentroidSet RandomInit()
        {
            CentroidSet startCentroidSet = new CentroidSet(Storage.s.DataSet.Count, Storage.s.DataPointDimensions, Storage.s.CentroidCount);

            for (int k = 0; k < Storage.s.CentroidCount; k++)
            {
                int index = GenuineRandomGenerator.GetInt(Storage.s.DataSet.Count);
                for (int n = 0; n < Storage.s.DataPointDimensions; n++)
                    startCentroidSet.Centroids[k][n] = Storage.s.DataSet[index].Features[n];
            }
            return startCentroidSet;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                var c = CentroidSetHistory[CentroidSetHistory.Count - 1].Clone();
                c.AssignmentStep(Storage.s.DataSet);
                c.MoveStep(Storage.s.DataSet);
                CentroidSetHistoryAdd(c);
            }            
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            var c = CentroidSetHistory[CentroidSetHistory.Count - 1].Clone();

            int i = GenuineRandomGenerator.GetInt(Storage.s.DataSet.Count);
            int k = GenuineRandomGenerator.GetInt(Storage.s.CentroidCount);
            for (int n = 0; n < Storage.s.DataPointDimensions; n++)
                c.Centroids[k][n] = Storage.s.DataSet[i].Features[n] + .1;
            CentroidSetHistoryAdd(c);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            double min = double.MaxValue;
            int index = -1;

            for (int i = 0; i < CentroidSetHistory.Count; i++)
            {
                double cost;
                if ((cost = (CentroidSetHistory[i].Cost(Storage.s.DataSet))) < min)
                {
                    min = cost;
                    index = i;
                }
            }
            CentroidSetHistoryAdd(Storage.s.CentroidSet = CentroidSetHistory[index].Clone());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CentroidSetHistoryAdd(RandomInit());
        }

        private void CentroidSetHistoryAdd(CentroidSet centroidSet)
        {
            centroidSet = centroidSet.Clone();
            chart1.Series[0].Points.Add(centroidSet.Cost(Storage.s.DataSet));
            CentroidSetHistory.Add(centroidSet);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var newCentroidSetHistory = new List<CentroidSet>();
            var last = CentroidSetHistory[CentroidSetHistory.Count - 1];
            chart1.Series[0].Points.Clear();
            CentroidSetHistory = newCentroidSetHistory;
            CentroidSetHistoryAdd(last);            
        }
    }
}
