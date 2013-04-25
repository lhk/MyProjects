using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperBot
{
    public static class GenuineRandomGenerator
    {
        static System.Security.Cryptography.RNGCryptoServiceProvider rng;
        static System.Security.Cryptography.RNGCryptoServiceProvider Rng
        {
            get { if (rng == null) rng = new System.Security.Cryptography.RNGCryptoServiceProvider(); return rng; }
        }

        public static ulong GetUInt64()
        {
            byte[] data = new byte[8];
            Rng.GetBytes(data);
            ulong result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                result |= ((ulong)data[i]) << (8 * i);
            }
            return result;
        }

        public static int GetInt()
        {
            ulong longrnd = GetUInt64();
            return (int)((long)(longrnd % (ulong)((long)int.MaxValue - (long)int.MinValue)) - (long)int.MinValue);
        }

        public static int GetInt(int max)
        {
            int i = GetInt();
            if (i < 0) i = -i;
            return i % max;
        }

        public static double GetDouble()
        {
            return ((double)GetUInt64()) / ((double)ulong.MaxValue);
        }

        public static float GetFloat() { return (float)GetDouble(); }

        public static int[] RandomMapping(int n)
        {
            List<int> randomMap = new List<int>(n);
            List<int> idMap = new List<int>(n);

            for (int i = 0; i < n; i++) idMap.Add(i);

            while (idMap.Count > 0)
            {
                int randomIndex = GetInt(idMap.Count);
                randomMap.Add(idMap[randomIndex]);
                idMap.RemoveAt(randomIndex);
            }
            return randomMap.ToArray();
        }
    }
}
