using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace PracticeTool
{
    public interface IPracticeModule
    {
        int Pages { get; }
        int ActivePage { get; set; }
        PointF MousePosition { set; }
        void Draw(Graphics g);
        void MouseDown(MouseButtons b);
        void MouseUp(MouseButtons b);
        ToolStripItem[] StripMenu { get; }
    }
}