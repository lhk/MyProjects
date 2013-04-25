using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PracticeTool
{
    public partial class PaintArea : UserControl
    {
        PointF transform;
        float scale;
        const float d_scale = 1.2f;
        bool MMB = false;
        PointF lastMousePoint;

        #region init
        public PaintArea()
        {
            InitializeComponent();
            MouseWheel += new MouseEventHandler(PaintArea_MouseWheel);
        }

        private void PaintArea_Load(object sender, EventArgs e)
        {
            scale = 0.7f;
            transform = new PointF(Width / 2 / scale, Height / 4 * 3 / scale);
        }
        #endregion


        #region render
        private void PaintArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(transform.X, transform.Y);

            // coordiante grid
            int n = 100000;
            int d = 100;
            if(scale > 0.05f) for (int i = -n; i < n; i += d)
            {
                Color c = Color.FromArgb(200, 200, 200);
                g.DrawLine(new Pen(c), -n, i, n, i);
                g.DrawLine(new Pen(c), i, -n, i, n);
            }
            for (int i = -n; i < n; i += 10*d)
            {
                Color c = Color.Black;
                g.DrawLine(new Pen(c), -n, i, n, i);
                g.DrawLine(new Pen(c), i, -n, i, n);
            }
            // origin
            g.FillEllipse(new SolidBrush(Color.Black), -5 / scale, -5 / scale, 10 / scale, 10 / scale);

            IPracticeModule module = null;
            try { module = ((Form1)Parent).ActivePracticeModule; }
            catch { }
            if (module != null) module.Draw(e.Graphics);
        }
        #endregion


        #region events
        void PaintArea_MouseWheel(object sender, MouseEventArgs e)
        {
            // zoom
            double min_scale = 0.018f, max_scale = 2;

            if ((e.Delta != 0) && (scale > min_scale && scale < max_scale || !(scale < min_scale ^ e.Delta > 0)))
            {
                double old_scale = scale;
                if (e.Delta > 0) scale *= d_scale;
                if (e.Delta < 0) scale /= d_scale;

                transform.X += (int)(e.X / scale - e.X / old_scale);
                transform.Y += (int)(e.Y / scale - e.Y / old_scale);

                Invalidate();
            }
        }

        private void PaintArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (MMB) // perspective shift
            {
                PointF d_mp = new PointF(0, 0);
                d_mp = new PointF(e.X - lastMousePoint.X, e.Y - lastMousePoint.Y);
                transform.X += (int)(d_mp.X / scale);
                transform.Y += (int)(d_mp.Y / scale);
            }
            lastMousePoint = e.Location;

            
            // screen to world - coordinate conversion
            PointF p = e.Location;
            p.X = (int)(p.X / scale);
            p.Y = (int)(p.Y / scale);
            p.X -= transform.X;
            p.Y -= transform.Y;
            ((Form1)Parent).ActivePracticeModule.MousePosition = p;
            Invalidate();
        }

        private void PaintArea_MouseLeave(object sender, EventArgs e)
        {
            ((Form1)Parent).ActivePracticeModule.MouseUp(MouseButtons.Left);
            ((Form1)Parent).ActivePracticeModule.MouseUp(MouseButtons.Right);
            Invalidate();
        }

        private void PaintArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) ((Form1)Parent).ActivePracticeModule.MouseDown(e.Button);
            if (e.Button == MouseButtons.Middle) MMB = true;
            Invalidate();
        }

        private void PaintArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) ((Form1)Parent).ActivePracticeModule.MouseUp(e.Button);
            if (e.Button == MouseButtons.Middle) MMB = false;
            Invalidate();
        }
        #endregion
    }
}
