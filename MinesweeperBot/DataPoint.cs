using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Data.SQLite;
using System.Data;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MinesweeperBot
{
    public class DataPoint : System.IEquatable<DataPoint>
    {
        double[] _features;
        public double[] Features
        {
            get { return _features; }
        }

        string _id;
        public string ID { get { return _id; } }

        char _label;
        public char Label { get { return _label; } set { _label = value;  } }
        char _prelabel;
        public char PreLabel { get { return _prelabel; } set { _prelabel = value;  } }

        const int digits = 3;

        char _set;
        /// <summary>
        /// t - trainings set
        /// v - cross validation set
        /// e - test set (evaluation)
        /// </summary>
        public char Set { get { return _set; } set { _set = value;  } }

        public DataPoint(string id, string features, char label, char preLabel, char set)
        {
            _id = id;
            _features = FormatHelper.StringToDoubleArray(features);
            _label = label;
            _prelabel = preLabel;
            _set = set;
        }

        public DataPoint(double[] features, char label, char preLabel, char set)
        {
            _features = features;
            _id = FormatHelper.hash(FormatHelper.DoubleArrayToString(_features, digits));
            _label = label;
            _prelabel = preLabel;
            _set = set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlite"></param>
        /// <returns>new entry => true, updated => false</returns>
        public bool SaveToDatabase(SQLiteConnection sqlite)
        {

            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LabeledData WHERE `ID` == @id";
            cmd.Parameters.Add(new SQLiteParameter("@id", ID));
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();
            bool ID_exists = reader.GetInt32(0) > 0;

            if (ID_exists)
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "UPDATE LabeledData SET `Label` = @label, `PreLabel` = @prelabel, `Set` = @set WHERE `ID` == @id";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@label", Label.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@prelabel", PreLabel.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@set", Set.ToString()));
                int count = cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "INSERT INTO LabeledData(`ID`, `Features`, `Label`, `PreLabel`, `Set`) VALUES (@id, @features, @label, @prelabel, @set)";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@features", FormatHelper.DoubleArrayToString(Features, digits)));
                cmd.Parameters.Add(new SQLiteParameter("@label", Label.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@prelabel", PreLabel.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@set", Set.ToString()));
                int count = cmd.ExecuteNonQuery();
            }


            return !ID_exists;
        }

        Bitmap drawCache = null;
        public void Draw(Graphics g, int offset_x, int offset_y)
        {
            if (drawCache == null)
            {
                drawCache = new Bitmap(16, 16);
                var g2 = Graphics.FromImage(drawCache);
                for (int x = 0; x < 16; x++) for (int y = 0; y < 16; y++)
                    {
                        int greyScale = (int)(Features[x * 16 + y] / 3);
                        Brush b = new SolidBrush(Color.FromArgb(greyScale, greyScale, greyScale));
                        g2.FillRectangle(b, x, y, 1, 1);
                    }
            }
            g.DrawImage(drawCache, offset_x, offset_y);

        }

        public bool Equals(DataPoint other)
        {
            return this.Equals((object)other);
        }

        public override bool Equals(object obj)
        {
            return
                this.ID == ((DataPoint)obj).ID;
        }

        public override int GetHashCode()
        {
            var s = ID;
            int res = 0;
            for (int i = 0; i < s.Length; i++)
            {
                res ^= ((int)s[i]) << ((i * 8) % 32);
            }
            return res;
        }
    }
}
