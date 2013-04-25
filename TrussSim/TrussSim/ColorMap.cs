using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace TrussSim
{
    public static class ColorMap
    {
        public static Color GetColor(double value, double min, double max)
        {
            value -= min;
            value /= (max - min);
            value = Math.Min(value, 1);
            value = Math.Max(value, 0);

            double R, G, B, x;

            int algoID = 0;

            switch (algoID)
            {
                case 1:
                    x = value;
                    R = 1 / (Math.Exp(-12 * (x - 0.8)) + 1);
                    G = 1 / (Math.Exp(-12 * (x - 0.5)) + 1);
                    B = 1 / (Math.Exp(-12 * (x - 0.2)) + 1);
                    break;


                default:
                    x = value * 3;
                    R = (x > 2) ? 1 : (x > 1) ? x - 1 : 0;
                    G = (x > 2) ? 3 - x : (x > 1) ? 1 : x;
                    B = (x > 2) ? 0 : (x > 1) ? 2 - x : 1;
                    double sum = (R + G + B) / 1.5;
                    R /= sum;
                    G /= sum;
                    B /= sum;

                    break;
            }

            R = Math.Min(R, 1);
            G = Math.Min(G, 1);
            B = Math.Min(B, 1);

            R = Math.Max(R, 0);
            G = Math.Max(G, 0);
            B = Math.Max(B, 0);

            return Color.FromArgb((int)(R * 255), (int)(G * 255), (int)(B * 255));
        }
    }
}
