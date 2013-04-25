using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Scalar = System.Double;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace TrussSim
{
    public partial class Form1 : Form
    {
        Truss truss = new Truss();
        HomogeneousProjection screenProjection = new HomogeneousProjection(2);
        Vector<Scalar> cameraPosition = new DenseVector(new Scalar[]{0,0});
        Scalar zoom = 1;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            truss.Calc();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateProjection();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            truss.Draw(g, screenProjection);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
                cameraPosition[0]++;

            else if (e.KeyCode == Keys.D)
                cameraPosition[0]--;

            else if (e.KeyCode == Keys.W)
                cameraPosition[1]--;

            else if (e.KeyCode == Keys.S)
                cameraPosition[1]++;

            else if (e.KeyCode == Keys.Q)
                zoom /= 1.2;

            else if (e.KeyCode == Keys.E)
                zoom *= 1.2;


            UpdateProjection();
        }

        private void UpdateProjection()
        {
            screenProjection.ProjectionMatrix =
                HomogeneousProjection.Translate(new DenseVector(new Scalar[] { ClientRectangle.Width / 2, ClientRectangle.Height / 2 })) *
                HomogeneousProjection.Scale(new DenseVector(new Scalar[] { 100, -100 })) *
                HomogeneousProjection.Scale(zoom,2) *
                HomogeneousProjection.Translate(-cameraPosition);

            Invalidate();
        }
    }
}
