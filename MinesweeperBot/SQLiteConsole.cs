using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MinesweeperBot
{
    public partial class SQLiteConsole : Form
    {
        public SQLiteConsole()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && textBox1.Text.Length > 0)
            {
                Storage.SQLite.Open();
                var sql_cmd = Storage.SQLite.CreateCommand();
                sql_cmd.CommandText = textBox1.Text;
                StringWriter writer = new StringWriter();
                try { 
                    var res = sql_cmd.ExecuteReader();
                    List<object[]> res2 = new List<object[]>();
                    while (res.Read())
                    {
                        object[] row = new object[10];
                        res.GetValues(row);
                        res2.Add(row);
                    }
                    if (res2.Count > 0) ObjectDumper.Write(res2, 10, writer);
                    else writer.Write("empty result");
                }
                catch (Exception ex) { writer.Write(ex.Message + "\n================================\n" + ex.StackTrace); }

                richTextBox1.Text += "\n================================\n" + writer.ToString();
                textBox1.Text = "";
                Storage.SQLite.Close();
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                textBox1.Focus();
            }
        }
    }
}
