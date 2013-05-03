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
    public partial class LabelData : Form
    {
        int lineWidth = 40;
        int startX = 50;
        int startY = 100;
        int stepXY = 25;
        Point mouseDown = new Point(-1, -1);
        Point mouseUp = new Point(-1, -1);
        Point mouseDownScreen = new Point(-1, -1);
        Point mouseMoveScreen = new Point(-1, -1);

        public LabelData()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            comboBox2.Items.Clear();
            foreach (var item in comboBox1.Items)
                comboBox2.Items.Add(item);

            comboBox1.SelectedIndex = comboBox2.SelectedIndex = 0;
        }



        private void LabelData_Paint(object sender, PaintEventArgs e)
        {
            lineWidth = (ClientSize.Width - 2 * startX) / stepXY;
            Graphics g = e.Graphics;
            var DataSet = Storage.DataPoints;

            int position = 0;
            bool labelChanged = false;

            if (mouseDownScreen.X >= 0) 
                g.DrawRectangle(new Pen(Color.Black, 2), Math.Min(mouseDownScreen.X, mouseMoveScreen.X), Math.Min(mouseDownScreen.Y, mouseMoveScreen.Y), Math.Abs(mouseDownScreen.X - mouseMoveScreen.X), Math.Abs(mouseDownScreen.Y - mouseMoveScreen.Y));

            for (int i = 0; i < DataSet.Count; i++)
            {
                if (comboBox1.SelectedIndex >= 0 && DataSet[i].Label == ((string)(comboBox1.Items[comboBox1.SelectedIndex]))[0])
                {
                    DataSet[i].Draw(g, startX + position % lineWidth * stepXY, startY + position / lineWidth * stepXY, 1);


                    // check selection
                    if (mouseDown.X >= 0 && mouseDown.Y >= 0 && mouseUp.X >= 0 && mouseUp.Y >= 0)
                    {
                        // check if current datapoint is in selection
                        if (Math.Min(mouseDown.X, mouseUp.X) <= position % lineWidth &&
                            Math.Max(mouseDown.X, mouseUp.X) >= position % lineWidth &&
                            Math.Min(mouseDown.Y, mouseUp.Y) <= position / lineWidth &&
                            Math.Max(mouseDown.Y, mouseUp.Y) >= position / lineWidth)
                        {
                            DataSet[i].Label = ((string)(comboBox2.Items[comboBox2.SelectedIndex]))[0];
                            labelChanged = true;
                        }
                    }
                    position++;
                }
            }
            if (labelChanged)
            {
                mouseDown = new Point(-1, -1);
                mouseUp = new Point(-1, -1);
                Invalidate();
            }

        }

        private void LabelData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = new Point((e.X - startX) / stepXY, (e.Y - startY) / stepXY);

                if (e.X >= startX && e.Y >= startY && p.X < lineWidth)
                {
                    mouseDown = p;
                    mouseDownScreen = e.Location;
                }
            }
            Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void LabelData_MouseMove(object sender, MouseEventArgs e)
        {
            Invalidate();
            mouseMoveScreen = e.Location;
        }

        private void LabelData_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = new Point((e.X - startX) / stepXY, (e.Y - startY) / stepXY);

                if (e.X >= startX && e.Y >= startY && p.X < lineWidth)
                {
                    mouseUp = p;
                    mouseDownScreen = new Point(-1, -1);
                    mouseMoveScreen = new Point(-1, -1);
                }
            }
            Invalidate();
        }

    }
}
