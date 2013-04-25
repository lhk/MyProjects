using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace PracticeTool.Modules
{
    class Dummy : IPracticeModule
    {
        public int Pages { get { return 2; } }
        public int ActivePage { get { return activePage; } set { activePage = value; } }
        int activePage = 1;
        public PointF MousePosition { set { mousePosition = value; } }
        PointF mousePosition;

        public void Draw(Graphics g)
        {
            if (activePage == 1) g.DrawString("Test test.", new System.Drawing.Font("Segoe UI", 12), new SolidBrush(Color.Black), new PointF(0, 0));
            else g.DrawString("Page 2.", new System.Drawing.Font("Segoe UI", 12), new SolidBrush(Color.Black), new PointF(0, 0));

            g.DrawEllipse(new Pen(Color.Black), this.mousePosition.X - 5, this.mousePosition.Y - 5, 10, 10);
        }

        public void MouseDown(MouseButtons b)
        {

        }

        public void MouseUp(MouseButtons b)
        {

        }


        public ToolStripItem[] StripMenu
        {
            get { throw new NotImplementedException(); }
        }
    }
}
