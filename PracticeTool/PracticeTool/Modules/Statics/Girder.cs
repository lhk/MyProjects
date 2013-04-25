using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
//using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace PracticeTool.Modules.Statics
{
    public class Girder : IPracticeModule
    {
        public int Pages { get { return 2; } }
        public int ActivePage { get; set; }
        public PointF MousePosition { set { mousePosition = value; } }
        public ToolStripItem[] StripMenu
        {
            get
            {
                ToolStripMenuItem i1 = new ToolStripMenuItem("Add Force");
                i1.Click += new EventHandler(addForce);
                return new ToolStripItem[] { i1 };
            }
        }

        PointF mousePosition;
        PointF lastMousePosition;
        Pen p3 = new Pen(Color.Black, 3);
        Pen p5 = new Pen(Color.Black, 5);
        Font font = new Font("Segoe UI", 22);

        Vector<double> drag_N_drop_Point;
        List<Vector<double>> drag_N_drop_Register = new List<Vector<double>>();

        double controlDragRadius = 15;

        // actual exercise related data
        List<Force> Loads;
        Vector<double> GirderEndPoint1, GirderEndPoint2, SeatedBearing, LooseBearing, SeatedBearingForce, LooseBearingForce;
        bool BearingAlignment;

        public Girder()
        {
            drag_N_drop_Register.Add(GirderEndPoint1 = MathHelper.Get2DVector(700, -300));
            drag_N_drop_Register.Add(GirderEndPoint2 = MathHelper.Get2DVector(-700, -300));
            drag_N_drop_Register.Add(SeatedBearing = MathHelper.Get2DVector(300, -300));
            drag_N_drop_Register.Add(LooseBearing = MathHelper.Get2DVector(-400, -300));
            BearingAlignment = true;

            Loads = new List<Force>();
            Loads.Add(new Force(MathHelper.Get2DVector(500, -700), MathHelper.Get2DVector(600, -500)));
            //Loads.Add(new Force(MathHelper.Get2DVector(300, 700), MathHelper.Get2DVector(300, 500)));

            SeatedBearingForce = MathHelper.Get2DVector(0, 0);
            LooseBearingForce = MathHelper.Get2DVector(0, 0);

            foreach (var item in Loads)
            {
                drag_N_drop_Register.Add(item.Base);
                drag_N_drop_Register.Add(item.Tip);
            }

            fixDNDControl(false);
        }

        #region UI stuff
        public void Draw(Graphics g)
        {
            PointF d_mousePosition = new PointF(mousePosition.X - lastMousePosition.X, mousePosition.Y - lastMousePosition.Y);
            lastMousePosition = mousePosition;

            if (drag_N_drop_Point != null)
            {
                drag_N_drop_Point[0] += d_mousePosition.X;
                drag_N_drop_Point[1] += d_mousePosition.Y;
                fixDNDControl(false);
            }

            foreach (var item in Loads) DrawArrow(g, item, true);

            // draw bearing
            if (ActivePage == 1)
            {
                drawBearing(g, SeatedBearing[0], SeatedBearing[1], BearingAlignment ? Math.Atan2(GirderEndPoint2[1] - GirderEndPoint1[1], GirderEndPoint2[0] - GirderEndPoint1[0]) : 0, false);
                drawBearing(g, LooseBearing[0], LooseBearing[1], BearingAlignment ? Math.Atan2(GirderEndPoint2[1] - GirderEndPoint1[1], GirderEndPoint2[0] - GirderEndPoint1[0]) : 0, true);
            }

            // draw girder
            g.DrawLine(p5, (float)GirderEndPoint1[0], (float)GirderEndPoint1[1], (float)GirderEndPoint2[0], (float)GirderEndPoint2[1]);


            // draw constroldrags
            if (ActivePage == 1)
            {
                DrawCircle(g, GirderEndPoint1, controlDragRadius);
                DrawCircle(g, GirderEndPoint2, controlDragRadius);
                DrawCircle(g, SeatedBearing, controlDragRadius);
                DrawCircle(g, LooseBearing, controlDragRadius);

                foreach (var item in Loads)
                {
                    DrawCircle(g, item.Base, controlDragRadius);
                    DrawCircle(g, item.Tip, controlDragRadius);
                }
            }

            // draw scale
            g.DrawLine(p3, 0, 50, 100, 50);
            g.DrawLine(p3, 0, 30, 0, 70);
            g.DrawLine(p3, 100, 30, 100, 70);


            // draw resulting forces
            DrawArrow(g, LooseBearing, LooseBearing + LooseBearingForce, false);
            DrawArrow(g, SeatedBearing, SeatedBearing + SeatedBearingForce, false);

            // draw labels
            g.DrawString("1 kN", font, new SolidBrush(Color.Black), 15, 0);
            g.DrawString("1 m", font, new SolidBrush(Color.Black), 15, 50);

            foreach (var load in Loads)
            {
                PointF p = PointF(load.Tip[0] - load.Base[0], load.Tip[1] - load.Base[1]);
                double len = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                p = PointF(p.X / 2 + p.Y / len * 100 + load.Base[0] - 50, -(p.Y / 2 - p.X / len * 50 + load.Base[1]));
                g.DrawString((len * 10).ToString("#") + " N", font, new SolidBrush(Color.Black), p.X, -p.Y);
            }

            // draw solns
            if (ActivePage == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector<double> Base = (i == 0) ? SeatedBearing : LooseBearing;
                    Vector<double> Vec = (i == 0) ? SeatedBearingForce : LooseBearingForce;
                    PointF p = PointF(Vec[0], Vec[1]);
                    double len = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                    p = PointF(p.X / 2 + p.Y / len * 100 + Base[0] - 50, -(p.Y / 2 - p.X / len * 50 + Base[1]));
                    g.DrawString("X: " + (Vec[0] * 10).ToString("0") + " N\nY: " + (-Vec[1] * 10).ToString("#") + " N", font, new SolidBrush(Color.Black), p.X, -p.Y);
                }
            }
        }

        PointF PointF(double x, double y) { return new System.Drawing.PointF((float)x, (float)y); }

        void drawBearing(Graphics g, double centerX, double centerY, double angle, bool loose)
        {
            Matrix transform = createTransform(centerX, centerY, angle);

            g.DrawLine(p3, doTransform(0, 0, transform), doTransform(-50, -100 + (loose ? 10 : 0), transform));
            g.DrawLine(p3, doTransform(0, 0, transform), doTransform(50, -100 + (loose ? 10 : 0), transform));
            if (loose) g.DrawLine(p3, doTransform(-50, -90, transform), doTransform(50, -90, transform));
            g.DrawLine(p3, doTransform(-100, -100, transform), doTransform(100, -100, transform));

            int limit = 6;
            double width = 200f / (limit + 2);
            for (int i = 0; i < limit; i++) g.DrawLine(p3, doTransform(width * (i + 2) - 100, -100, transform), doTransform(width * (i + 1) - 100, -100 - width, transform));
        }

        Matrix createTransform(double x, double y, double angle)
        {
            Matrix rotation = new DenseMatrix(new double[,] { { Math.Cos(angle), -Math.Sin(angle), 0 }, { Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 1 } });
            Matrix translation = new DenseMatrix(new double[,] { { 1, 0, x }, { 0, 1, y }, { 0, 0, 1 } });
            Matrix transform = (Matrix)((Matrix<double>)translation * (Matrix<double>)rotation);
            return transform;
        }

        PointF doTransform(double x, double y, Matrix transform)
        {
            var v = transform.Multiply(new DenseVector(new double[] { x, y, 1 }));
            return PointF(v[0], v[1]);
        }

        void fixDNDControl(bool alignment)
        {
            if (drag_N_drop_Point != null && alignment)
            {
                drag_N_drop_Point[0] = Math.Round(drag_N_drop_Point[0] / 100) * 100;
                drag_N_drop_Point[1] = Math.Round(drag_N_drop_Point[1] / 100) * 100;
            }
            if (GirderEndPoint2[0] - GirderEndPoint1[0] != 0)
            {
                double slope = (GirderEndPoint2[1] - GirderEndPoint1[1]) / (GirderEndPoint2[0] - GirderEndPoint1[0]);
                SeatedBearing[1] = (SeatedBearing[0] - GirderEndPoint1[0]) * slope + GirderEndPoint1[1];
                LooseBearing[1] = (LooseBearing[0] - GirderEndPoint1[0]) * slope + GirderEndPoint1[1];
            }
            solveBalanceConditions();
        }

        void DrawArrow(Graphics g, Force f, bool ProjectToGirder)
        {
            DrawArrow(g, f.Base, f.Tip, ProjectToGirder);
        }

        void DrawArrow(Graphics g, Vector<double> Base, Vector<double> Tip, bool ProjectToGirder)
        {
            if (Base[0] == Tip[0] && Base[1] == Tip[1]) return;
            Vector<double> intersectionPoint = new DenseVector(2);
            MathHelper.RayIntersection(Base, Tip, GirderEndPoint1, GirderEndPoint2, ref intersectionPoint);

            Matrix transform;
            double lengthBase = (intersectionPoint.Subtract(Base).Norm(2));
            double lengthTip = (intersectionPoint.Subtract(Tip).Norm(2));
            double length = (Base.Subtract(Tip).Norm(2));
            if (!ProjectToGirder)
            {
                // arrow base to tip
                transform = createTransform(Tip[0], Tip[1], Math.Atan2(-Tip[1] + Base[1], -Tip[0] + Base[0]));
            }
            else if (lengthBase > lengthTip)
            {
                // arrow base to intersectionpoint
                transform = createTransform(intersectionPoint[0], intersectionPoint[1], Math.Atan2(-intersectionPoint[1] + Base[1], -intersectionPoint[0] + Base[0]));
                length = lengthBase;
            }
            else
            {
                // arrow intersectionpoint to tip
                transform = createTransform(Tip[0], Tip[1], Math.Atan2(-Tip[1] + intersectionPoint[1], -Tip[0] + intersectionPoint[0]));
                length = lengthTip;
            }
            try
            {
                g.DrawLine(p3, doTransform(0, 0, transform), doTransform(length, 0, transform));
                g.DrawLine(p3, doTransform(controlDragRadius, 0, transform), doTransform(3 * controlDragRadius, 10, transform));
                g.DrawLine(p3, doTransform(controlDragRadius, 0, transform), doTransform(3 * controlDragRadius, -10, transform));
            }
            catch { }
        }


        void DrawCircle(Graphics g, Vector<double> v, double radius)
        {
            DrawCircle(g, v[0], v[1], radius);
        }

        void DrawCircle(Graphics g, double centerX, double centerY, double radius)
        {
            g.FillEllipse(new SolidBrush(Color.White), (float)(centerX - radius), (float)(centerY - radius), (float)(2 * radius), (float)(2 * radius));
            g.DrawEllipse(p3, (float)(centerX - radius), (float)(centerY - radius), (float)(2 * radius), (float)(2 * radius));
        }
        #endregion

        public void MouseDown(MouseButtons b)
        {
            // activate drag n drop on leftclick
            if (b == MouseButtons.Left && ActivePage == 1)
            {
                foreach (var item in drag_N_drop_Register) if (MathHelper.Get2DVector(mousePosition.X, mousePosition.Y).Subtract(item).Norm(2) < controlDragRadius)
                    { drag_N_drop_Point = item; break; }
            }
            // delete load on rightclick
            if (b == MouseButtons.Right && ActivePage == 1)
            {
                for (int i = 0; i < Loads.Count; i++)
                    if ((MathHelper.Get2DVector(mousePosition.X, mousePosition.Y).Subtract(Loads[i].Tip).Norm(2) < controlDragRadius)
                        || (MathHelper.Get2DVector(mousePosition.X, mousePosition.Y).Subtract(Loads[i].Base).Norm(2) < controlDragRadius))
                    {
                        drag_N_drop_Register.Remove(Loads[i].Tip);
                        drag_N_drop_Register.Remove(Loads[i].Base);
                        Loads.RemoveAt(i);
                        fixDNDControl(false);
                        break;
                    }
            }
        }

        public void MouseUp(MouseButtons b)
        {
            if (b == MouseButtons.Left)
            {
                fixDNDControl(true);
                drag_N_drop_Point = null;
            }
        }

        void addForce(object sender, EventArgs e)
        {
            Loads.Add(new Force(MathHelper.Get2DVector(0, -700), MathHelper.Get2DVector(0, -500)));
            drag_N_drop_Register.Add(Loads[Loads.Count - 1].Base);
            drag_N_drop_Register.Add(Loads[Loads.Count - 1].Tip);
            solveBalanceConditions();
            Form1.instance.Invalidate();
        }

        void solveBalanceConditions()
        {
            double sumBendingMoment = 0;
            Vector<double> centerOfMoment = SeatedBearing;
            Vector<double> sumForce = new DenseVector(2);

            foreach (var load in Loads)
            {
                Vector<double> force = load.Tip - load.Base;
                Vector<double> RadiusToCenterOfRotation = new DenseVector(2);
                MathHelper.PointRayPerpendicularPoint(load.Base, load.Tip, centerOfMoment, ref RadiusToCenterOfRotation);
                RadiusToCenterOfRotation = RadiusToCenterOfRotation - centerOfMoment;
                sumForce += force;
                // vec(M) = vec(r) x vec(F)
                sumBendingMoment += RadiusToCenterOfRotation[0] * force[1] - RadiusToCenterOfRotation[1] * force[0];
            }

            double angle = Math.Atan2(GirderEndPoint2[1] - GirderEndPoint1[1], GirderEndPoint2[0] - GirderEndPoint1[0]);
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            Vector<double> tmp = new DenseVector(new double[] { -sin, cos });
            tmp += LooseBearing;
            MathHelper.PointRayPerpendicularPoint(LooseBearing, tmp, centerOfMoment, ref tmp);
            Vector<double> Radius = tmp - centerOfMoment;

            // mat(A) * vec(X) = vec(B)
            Matrix<double> eqnsA = new DenseMatrix(new double[,]
            {
                {1,0,-sin},
                {0,1,cos},
                {0,0,Radius[0]*cos+Radius[1]*sin},
            });
            Vector<double> equnsB = new DenseVector(new double[] { -sumForce[0], -sumForce[1], -sumBendingMoment });
            Vector<double> solns = MathHelper.Solve(eqnsA, equnsB);
            SeatedBearingForce[0] = solns[0];
            SeatedBearingForce[1] = solns[1];
            LooseBearingForce[0] = solns[2] * (-sin);
            LooseBearingForce[1] = solns[2] * cos;
        }

    }

    public class Force
    {
        public Vector<double> Base, Tip;
        public Force(Vector<double> _Base, Vector<double> _Tip) { Base = _Base; Tip = _Tip; }
    }
}
