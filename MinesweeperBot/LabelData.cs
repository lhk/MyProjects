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
        Point click = new Point(-1, -1);

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
            var DataSet = Storage.s.DataSet;

            int position = 0;

            for (int i = 0; i < DataSet.Count; i++)
            {
                if (comboBox1.SelectedIndex >= 0 && DataSet[i].Label == ((string)(comboBox1.Items[comboBox1.SelectedIndex]))[0])
                {
                    DataSet[i].Draw(g, startX + position % lineWidth * stepXY, startY + position / lineWidth * stepXY, 1);

                    if(click.X==position % lineWidth && click.Y==position / lineWidth)
                    {
                        if (comboBox2.SelectedIndex >= 0)
                            DataSet[i].Label = ((string)(comboBox2.Items[comboBox2.SelectedIndex]))[0];
                        click = new Point(-1, -1);
                        Invalidate();
                        return;
                    }
                    position++;
                }
            }

        }

        private void LabelData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                click = new Point((e.X - startX) / stepXY, (e.Y - startY) / stepXY);

                if (e.X >= startX &&
                    e.Y >= startY &&
                    click.X < lineWidth)
                {

                }
                else click = new Point(-1, -1);
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

        private void button1_Click(object sender, EventArgs e)
        {
            var newList = new List<DataPoint>();
            for (int i = 0; i < Storage.s.DataSet.Count; i++)
            {
                if (comboBox1.SelectedIndex >= 0 && Storage.s.DataSet[i].Label != ((string)(comboBox1.Items[comboBox1.SelectedIndex]))[0])
                {
                    newList.Add(Storage.s.DataSet[i]);
                }
            }
            Storage.s.DataSet = newList;
            Invalidate();
        }
    }
}
