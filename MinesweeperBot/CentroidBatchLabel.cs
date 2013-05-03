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
    public partial class CentroidBatchLabel : Form
    {
        public CentroidBatchLabel()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            //KMeans.AssignmentStep();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void DataManager_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int position = 0;
            for (int i = 0; i < Storage.DataPoints.Count; i++) if(Storage.s.CentroidSet.DataCentroidConnection[i] == numericUpDown1.Value)
            {
                Storage.DataPoints[i].Draw(g, (position % 40) * 30 + 50, (position / 40) * 30 + 50, 1);
                position++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BatchLabel();           
        }

        private void BatchLabel()
        {
            if (textBox1.Text.Length == 1)
            {
                char c = textBox1.Text[0];
                for (int i = 0; i < Storage.DataPoints.Count; i++)
                    if (Storage.s.CentroidSet.DataCentroidConnection[i] == numericUpDown1.Value)
                        Storage.DataPoints[i].Label = c;
            }
            
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                BatchLabel();
                numericUpDown1.Value++;
                textBox1.Text = "";
            }
        }
    }
}
