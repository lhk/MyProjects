using System;
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
using MySql.Data.MySqlClient;

namespace MinesweeperBot
{
    public partial class Main : Form
    {
        public static Main _Main;
        //public List<DataPoint> Storage.s.DataSet = new List<DataPoint>();
        //string Storage.s.DataSetFile = "D:\\Storage.s.DataSet.xml";
        string StorageFile = Program.GetBasePath() + "\\Storage.xml";

        MySqlConnection MySqlConnection;
        string MySqlConnectionString;

        public Main()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;

            // format: Server=ip/host;Database=usr_web464_1;Uid=name;Pwd=esrtgq34ta;
            using (TextReader reader = File.OpenText(@"D:\MinesweeperBotMySqlLogin.txt"))
                MySqlConnectionString = reader.ReadToEnd();

            MySqlConnection = new MySqlConnection(MySqlConnectionString);
            var cmd = MySqlConnection.CreateCommand();
            cmd.CommandText = "SHOW TABLES";
            var res = cmd.ExecuteReader();

            Storage.Load(StorageFile);
            _Main = this;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Storage.Save(StorageFile);
        }

        private void register(Form f)
        {
            f.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            register(new MinesweeperInterface());
        }      

        private void button2_Click(object sender, EventArgs e)
        {
            register(new KMeans());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            register(new CentroidBatchLabel());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            register(new LabelData());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            register(new SupervisedLearningAlgo());            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            register(new MySQLConsole());
        }
    }
}
