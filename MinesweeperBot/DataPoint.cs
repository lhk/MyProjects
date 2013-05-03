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

namespace MinesweeperBot
{
    /*public class DataPoint : System.IEquatable<DataPoint>
    {
        public double[] Features { get; set; }

        public char Label = '?';


        public bool Equals(DataPoint other)
        {
            return this.Equals((object)other);
        }

        public override bool Equals(object obj)
        {
            DataPoint d2 = obj as DataPoint;
            if (d2 == null) return false;

            bool equals = true;
            for (int n = 0; n < Features.Length; n++)
            {
                if (d2.Features[n] != this.Features[n])
                {
                    equals = false;
                    break;
                }
            }
            return equals;
        }

        public override int GetHashCode()
        {

            double sum = 0;
            for (int n = 0; n < Features.Length; n++)
            {
                double x = Math.Abs(Features[n]) + 1;
                sum += 100 * x / Math.Pow(10, Math.Floor(Math.Log10(x)));
            }

            return (int)sum;
        }

        internal void Draw(Graphics g, int offset_x, int offset_y, int scale)
        {
            for (int x = 0; x < 16; x++) for (int y = 0; y < 16; y++)
                {
                    Brush b = new SolidBrush(Color.FromArgb(
                        (int)(Features[x * 16 + y] / 3),
                        (int)(Features[x * 16 + y] / 3),
                        (int)(Features[x * 16 + y] / 3)
                        ));
                    g.FillRectangle(b, offset_x + scale * x, offset_y + scale * y, scale, scale);
                }
        }
    }*/


    public class DataPoint
    {
        double[] _features;
        public double[] Features { get { return _features; } }

        string _id;
        public string ID { get { return _id; } }

        public char Label;
        public char PreLabel;

        const int digits = 3;

        /// <summary>
        /// t - trainings set
        /// v - cross validation set
        /// e - test set
        /// </summary>
        public char Set;

        public DataPoint(string id, string features, char label, char preLabel, char set)
        {
            _id = id;
            _features = FormatHelper.StringToDoubleArray(features);
            Label = label;
            PreLabel = preLabel;
            Set = set;
        }

        public DataPoint(double[] features, char label, char preLabel, char set)
        {
            _features = features;
            _id = FormatHelper.hash(FormatHelper.DoubleArrayToString(_features, digits));
            Label = label;
            PreLabel = preLabel;
            Set = set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlite"></param>
        /// <returns>new entry => true, updated => false</returns>
        public bool SaveToDatabase(SQLiteConnection sqlite, bool openCloseDB)
        {
            if (openCloseDB) sqlite.Open();


            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LabeledData WHERE ID == @id";
            cmd.Parameters.Add(new SQLiteParameter("@id", ID));
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();
            bool ID_exists = reader.GetInt32(0) > 0;

            if (ID_exists)
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "UPDATE LabeledData SET 'Label' = @label, 'PreLabel' = @prelabel, 'Set' = @set WHERE 'ID' == @id";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@label", Label.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@prelabel", PreLabel.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@set", Set.ToString()));
                int count = cmd.ExecuteNonQuery();
                if (count > 0)
                {

                }
            }
            else
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "INSERT INTO LabeledData('ID', 'Features', 'Label', 'PreLabel', 'Set') VALUES (@id, @features, @label, @prelabel, @set)";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@features", FormatHelper.DoubleArrayToString(Features, digits)));
                cmd.Parameters.Add(new SQLiteParameter("@label", Label.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@prelabel", PreLabel.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@set", Set.ToString()));
                int count = cmd.ExecuteNonQuery();
            }

            if (openCloseDB) sqlite.Close();

            return !ID_exists;
        }
        public bool SaveToDatabase(SQLiteConnection sqlite) { return SaveToDatabase(sqlite, true); }

        public void Draw(Graphics g, int offset_x, int offset_y, int scale)
        {
            for (int x = 0; x < 16; x++) for (int y = 0; y < 16; y++)
                {
                    Brush b = new SolidBrush(Color.FromArgb(
                        (int)(Features[x * 16 + y] / 3),
                        (int)(Features[x * 16 + y] / 3),
                        (int)(Features[x * 16 + y] / 3)
                        ));
                    g.FillRectangle(b, offset_x + scale * x, offset_y + scale * y, scale, scale);
                }
        }
    }
}
