using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;

namespace MinesweeperBot
{
    public class DataPoint : System.IEquatable<DataPoint>
    {
        public double[] Features { get; set; }

        public char Label = '?';

        //[XmlIgnore]
        //public int KMeansCentroidID = -1;







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
    }
}
