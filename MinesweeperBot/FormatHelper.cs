using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperBot
{
    public static class FormatHelper
    {
        public static string DoubleArrayToString(double[] v, int digits)
        {
            digits = Math.Min(18, Math.Max(1, digits));
            StringBuilder s = new StringBuilder();
            foreach (var x in v) s.Append(x.ToString("E"+digits.ToString(), System.Globalization.CultureInfo.InvariantCulture)+";");
            return s.ToString();
        }

        public static double[] StringToDoubleArray(string s)
        {
            var s2 = s.Split(';');
            List<double> v = new List<double>();
            foreach (var x_encoded in s2)
            {
                double x;
                if (double.TryParse(x_encoded, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x))
                {
                    v.Add(x);
                }
            }
            return v.ToArray();
        }

        public static string hash(string input)
        {
            var hashFunc = System.Security.Cryptography.SHA512.Create();
            byte[] hashBytes = hashFunc.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            byte[] hashBytesShort = new byte[9];
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashBytesShort[i % hashBytesShort.Length] ^= hashBytes[i];
            }
            return Convert.ToBase64String(hashBytesShort);
        }
    }
}
