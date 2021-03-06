﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Data.SQLite;

namespace MinesweeperBot
{
    public partial class Main : Form
    {
        public static Main _Main;

        public Main()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            Storage.Load();
            _Main = this;

        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Storage.Save();
        }

        private void register(Form f)
        {
            f.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            register(new MinesweeperInterface());
        }      

        private void button4_Click(object sender, EventArgs e)
        {
            register(new LabelData());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            register(new SupervisedLearning());            
        }

    }
}
