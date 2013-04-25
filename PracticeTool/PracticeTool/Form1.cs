using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PracticeTool
{
    public partial class Form1 : Form
    {
        public static Form1 instance;
        public IPracticeModule ActivePracticeModule;

        public Form1()
        {
            InitializeComponent();
            LoadModule(new Modules.Statics.Girder());
            WindowState = FormWindowState.Maximized;
            instance = this;
        }

        private void LoadModule(IPracticeModule module)
        {
            ActivePracticeModule = module;
            hScrollBar1.Minimum = 1;
            hScrollBar1.Maximum = module.Pages;
            hScrollBar1.Value = 1;
            module.ActivePage = 1;
            label1.Text = "Page 1/" + hScrollBar1.Maximum;
            moduleMenuToolStripMenuItem.DropDownItems.Clear();
            moduleMenuToolStripMenuItem.DropDownItems.AddRange(ActivePracticeModule.StripMenu);
            

            paintArea1.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    try { hScrollBar1.Value--; paintArea1.Invalidate(); }
                    catch { } 
                    break;
                case Keys.Right:
                    try { hScrollBar1.Value++; paintArea1.Invalidate(); }
                    catch { } 
                    break;
                case Keys.Up: break;
                case Keys.Down: break;

                default: return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;  // used
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            label1.Text = "Page " + hScrollBar1.Value + "/" + hScrollBar1.Maximum;
            ActivePracticeModule.ActivePage = hScrollBar1.Value;
            paintArea1.Invalidate();
        }
        
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label1.Text = "Page " + hScrollBar1.Value + "/" + hScrollBar1.Maximum;
            ActivePracticeModule.ActivePage = hScrollBar1.Value;
            paintArea1.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            paintArea1.Invalidate();
        }

    }
}
