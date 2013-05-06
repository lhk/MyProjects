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


    public class DataPoint : System.IEquatable<DataPoint>
    {
        double[] _features;
        public double[] Features { get { return _features; } }

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
        //public bool SaveToDatabase(SQLiteConnection sqlite) { return SaveToDatabase(sqlite, true); }
        //public bool SaveToDatabase() { return SaveToDatabase(Storage.SQLite); }

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
            var s = FormatHelper.DoubleArrayToString(_features, digits);
            int res = 0;
            for (int i = 0; i < s.Length; i++)
            {
                res ^= ((int)s[i]) << ((i * 8) % 32);
            }
            return res;
        }

        /*Vector<double> ReducedFeatures = null;
        public Vector<double> GetReducedFeatures()
        {
            if (ReducedFeatures == null) ReducedFeatures = Storage.PCA.EvaluateFunction(new DenseVector(Features));
            return ReducedFeatures;
        }*/

        public static Vector<double> Preprocess(double[] features)
        {
            Vector<double> output = new DenseVector(features.Length);
            double min = double.MaxValue, max = double.MinValue, mean = 0;
            for (int i = 0; i < features.Length; i++)
            {
                min = Math.Min(min, features[i]);
                max = Math.Max(max, features[i]);
                mean += features[i];
            }
            mean /= features.Length;

            if (max == min) max += 1e-8;

            for (int i = 0; i < features.Length; i++)
            {
                output[i] = (features[i] - mean) / (max - min);
            }
            return output;
        }
    }
}
