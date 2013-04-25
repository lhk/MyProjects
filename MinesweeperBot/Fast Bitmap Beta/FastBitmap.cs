using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

namespace bmp
{
	/// <summary>
	/// Diese Klasse ist eine Verbesserung der System.Drawing.Bitmap-klasse
	/// Verbesserung meint: schnellere Arbeit, größerer Funktions-Umfang
	/// </summary>
    public class FastBitmap
    {
    	#region felder
    	bool Ready = false;
        private byte[] bildDaten;
        private Color[,] color;
        private int width;
        private int height;
        Bitmap Bild;
        Rectangle rect;
        bool modified;
        int bytes;
        int stride;
        System.Drawing.Imaging.PixelFormat pixelFormat;
        System.Drawing.Imaging.ColorPalette colorPalette;
        #endregion
        #region konstruktoren und zubehör
        public FastBitmap(Bitmap bild)
        {
            Bild = bild;
            new Thread(new ThreadStart(SetUp)).Start();
        }

        public FastBitmap(int width, int height):this(new Bitmap(width,height)){}

        public FastBitmap(System.Drawing.Size s) : this(s.Width, s.Height) { }

        public FastBitmap(int width, int height, System.Drawing.Imaging.PixelFormat p) : this(new Bitmap(width, height,p)) { }

        public FastBitmap(System.Drawing.Size s,System.Drawing.Imaging.PixelFormat p) : this(s.Width, s.Height,p) { }
        
        void SetUp()
        {
            colorPalette = Bild.Palette;
            pixelFormat = Bild.PixelFormat;
            width = Bild.Width;
            height = Bild.Height;
            rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData bmpData =
                Bild.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                Bild.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            stride = bmpData.Stride;
            bytes = stride * height;
            bildDaten = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bildDaten, 0, bytes);
            Bild.UnlockBits(bmpData);
            color = new Color[width, height];
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    Format32BppArgb();
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    Format24BppRgb();
                    break;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    Format8BppIndexed();
                    break;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    Format4BppIndexed();
                    break;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    Format1BppIndexed();
                    break;
            }
            modified = false;
            Ready = true;
       }
        
        void Format32BppArgb()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    color[x, y] = Color.FromArgb(bildDaten[y * stride + x * 4 + 3], bildDaten[y * stride + x * 4 + 2], bildDaten[y * stride + x * 4 + 1], bildDaten[y * stride + x * 4]);
        }
 
        void Format24BppRgb()
        {
             for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    color[x, y] = Color.FromArgb(bildDaten[y * stride + x * 3 + 2], bildDaten[y * stride + x * 3 + 1], bildDaten[y * stride + x * 3]);
        }
        
        void Format8BppIndexed()
        {
             for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    color[x, y] = colorPalette.Entries[bildDaten[y * stride + x]];
        }
        
        void Format4BppIndexed()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (x % 2 == 0)
                        color[x, y] = colorPalette.Entries[LowByte(bildDaten[y * stride + x / 2])];
                    else
                        color[x, y] = colorPalette.Entries[HighByte(bildDaten[y * stride + x / 2])];
        }
        
        void Format1BppIndexed()
        {
            int rest = width % 8;
            byte bits;
            int x, y;
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width - 8; x += 8)
                {
                    bits = bildDaten[y * stride + x / 8];
                    color[x, y] = colorPalette.Entries[(bits & 128) / 128];
                    color[x + 1, y] = colorPalette.Entries[(bits & 64) / 64];
                    color[x + 2, y] = colorPalette.Entries[(bits & 32) / 32];
                    color[x + 3, y] = colorPalette.Entries[(bits & 16) / 16];
                    color[x + 4, y] = colorPalette.Entries[(bits & 8) / 8];
                    color[x + 5, y] = colorPalette.Entries[(bits & 4) / 4];
                    color[x + 6, y] = colorPalette.Entries[(bits & 2) / 2];
                    color[x + 7, y] = colorPalette.Entries[bits & 1];
                }
                bits = bildDaten[y * stride + x / 8];
                int teiler = 128;
                for (int i = 0; i < rest; i++)
                {
                    color[x + i, y] = colorPalette.Entries[(bits & teiler) / teiler];
                    teiler /= 2;
                }
            }
        }
        
        int HighByte(byte zahl)
        {
            return zahl >> 4;
        }
        
        int LowByte(byte zahl)
        {

            return zahl & 15;
        }
        #endregion
        #region public-member

        /// <summary>
        /// Gibt die Farbe eines Pixels an der angegebenen Poisition an
        /// </summary>
        /// <param name="p">Poisition des Pixels</param>
        /// <returns>Die Farbe des Pixels</returns>
        public Color GetPixel(Point p)
        {
            return GetPixel(p.X, p.Y);
        }

        /// <summary>
        /// Gibt die Farbe eines Pixels an der angegebenen Poisition an
        /// </summary>
        /// <param name="x">X-Koordinate des Pixels</param>
        /// <param name="y">Y-Koordinate des Pixels</param>
        /// <returns>Die Farbe des Pixels</returns>
        public Color GetPixel(int x, int y)
        {
        	while(!Ready){}
            return color[x, y];
        }

        /// <summary>
        /// Setzt den Pixel an der angegeben Poisition auf die angegebene Farbe
        /// </summary>
        /// <param name="x">X-Koordinate des Pixels</param>
        /// <param name="y">Y-Koordinate des Pixels</param>
        /// <param name="col">Die neue Farbe des Pixels</param>
        public void SetPixel(int x, int y, Color col)
        {
        	while(!Ready){}
            color[x, y] = col;
            modified = true;
        }

        /// <summary>
        /// Setzt den Pixel an der angegeben Poisition auf die angegebene Farbe
        /// </summary>
        /// <param name="p">Poisition des Pixels</param>
        /// <param name="col"></param>
        public void SetPixel(Point p, Color col)
        {
            SetPixel(p.X, p.Y, col);
        }

        /// <summary>
        /// Ersetzt alle Pixel der angegeben Farbe durch eine andere Farbe
        /// </summary>
        /// <param name="old_color">Diese Farbe wird ersetzt</param>
        /// <param name="new_color">Ersetzt old_color</param>
        /// <param name="range">Schließt auch Pixel ein, die nicht genau die gleiche farbe haben</param>
        public void ReplacePixel(Color old_color, Color new_color, byte range)
        {
            while(!Ready){}
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color c = GetPixel(x, y);
                    if ((c.A + range >= old_color.A && c.A - range <= old_color.A)&&
                        (c.R + range >= old_color.R && c.R - range <= old_color.R)&&
                        (c.G + range >= old_color.G && c.G - range <= old_color.G)&&
                        (c.B + range >= old_color.B && c.B - range <= old_color.B)) 
                        SetPixel(x, y, new_color);
                }
            }
        }

        /// <summary>
        /// Ersetzt alle Pixel der angegeben Farbe durch eine andere Farbe
        /// </summary>
        /// <param name="old_color">Diese Farbe wird ersetzt</param>
        /// <param name="new_color">Ersetzt old_color</param>
        public void ReplacePixel(Color old_color, Color new_color)
        {
            ReplacePixel(old_color, new_color, 0);
        }
        
        /// <summary>
        /// Gibt die Breite des Bilds an
        /// </summary>
        public int Width
        {
            get {while(!Ready){} return width; }
        }
        
        /// <summary>
        /// Gibt die Höhe des Bilds an
        /// </summary>
        public int Height
        {
            get {while(!Ready){} return height; }
        }

        /// <summary>
        /// Gibt ein 2-Dimensionales Color Array zurück, welches das Bild darstellt
        /// </summary>
        public Color[,] ToColorArray()
        {
            while (!Ready) { }
            return color;
        }
        
        /// <summary>
        /// Konvertiert das Bild in eine System.Drawing.Bitmap-Inztanz
        /// </summary>
        public Bitmap ToBitmap()
        {
        	while(!Ready){}
                if (!modified) return Bild;
                switch (pixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        return ReturnFormat32BppArgb();
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        return ReturnFormat24BppRgb();
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        return ReturnFormat8BppIndexed();
                    case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                        return ReturnFormat4BppIndexed();
                    case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                        return ReturnFormat1BppIndexed();
                }
                throw new Exception("interner Fehler");
            }

        #endregion
        #region returns
        Bitmap ReturnFormat32BppArgb()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    bildDaten[y * stride + x * 4 + 3] = color[x, y].A;
                    bildDaten[y * stride + x * 4 + 2] = color[x, y].R;
                    bildDaten[y * stride + x * 4 + 1] = color[x, y].G;
                    bildDaten[y * stride + x * 4] = color[x, y].B;
                }
            System.Drawing.Imaging.BitmapData bmpData =
               Bild.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
               Bild.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(bildDaten, 0, ptr, bytes);
            Bild.UnlockBits(bmpData);
            modified = false;
            return Bild;
        }

        Bitmap ReturnFormat24BppRgb()
        {
        	for (int y = 0; y < height; y++)
        	{
                for (int x = 0; x < width; x++)
                {
                    bildDaten[y * stride + x * 3 + 2] = color[x, y].R;
                    bildDaten[y * stride + x * 3 + 1] = color[x, y].G;
                    bildDaten[y * stride + x * 3] = color[x, y].B;
                }
        	}
            System.Drawing.Imaging.BitmapData bmpData =
               Bild.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
               Bild.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(bildDaten, 0, ptr, bytes);
            Bild.UnlockBits(bmpData);
            modified = false;
            return Bild;
        }

        Bitmap ReturnFormat8BppIndexed()
        {
        	throw new Exception("Bild-Format noch nicht unterstützt\ndies ist nur eine Beta");
        }
        
        Bitmap ReturnFormat4BppIndexed()
        {
        	throw new Exception("Bild-Format noch nicht unterstützt\ndies ist nur eine Beta");
        }
        
        Bitmap ReturnFormat1BppIndexed()
        {
        	throw new Exception("Bild-Format noch nicht unterstützt\ndies ist nur eine Beta");
        }
        #endregion
    }

    /// <summary>
    /// Ermöglicht Farbkorektruren der Helligkeit und Sättigung
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HsvColor
    {
        int h;
        int s;
        int v;
        /// <summary>
        /// Farbton in Grad°
        /// </summary>
        public int Hue
        {
            get { return h; }
            set { h = (value >= 0 && value <= 360) ? value : 0; }
        }
        /// <summary>
        /// Sättigung auf 100 nominiert
        /// </summary>
        public int Saturation
        {
            get { return s; }
            set { s = (value >= 0 && value <= 100) ? value : 100; }
        }
        /// <summary>
        /// Helligkeit auf 100 nominiert
        /// </summary>
        public int Value
        {
            get { return v; }
            set { v = (value >= 0 && value <= 100) ? value : 100; }
        }

        public HsvColor(int hue, int saturation, int value)
        {
            if ((hue < 0) || (hue > 360))
            {
                throw new ArgumentOutOfRangeException("hue", "must be in the range [0, 360]");
            }
            if ((saturation < 0) || (saturation > 100))
            {
                throw new ArgumentOutOfRangeException("saturation", "must be in the range [0, 100]");
            }
            if ((value < 0) || (value > 100))
            {
                throw new ArgumentOutOfRangeException("value", "must be in the range [0, 100]");
            }
            this.h = hue;
            this.s = saturation;
            this.v = value;
        }

        public static HsvColor FromColor(Color color)
        {
            double num7;
            double num8;
            double num4 = ((double)color.R) / 255;
            double num5 = ((double)color.G) / 255;
            double num6 = ((double)color.B) / 255;
            double num = Math.Min(Math.Min(num4, num5), num6);
            double num2 = Math.Max(Math.Max(num4, num5), num6);
            double num9 = num2;
            double num3 = num2 - num;
            if ((num2 == 0) || (num3 == 0))
            {
                num8 = 0;
                num7 = 0;
            }
            else
            {
                num8 = num3 / num2;
                if (num4 == num2)
                {
                    num7 = (num5 - num6) / num3;
                }
                else if (num5 == num2)
                {
                    num7 = 2 + ((num6 - num4) / num3);
                }
                else
                {
                    num7 = 4 + ((num4 - num5) / num3);
                }
            }
            num7 *= 60;
            if (num7 < 0)
            {
                num7 += 360;
            }
            return new HsvColor((int)num7, (int)(num8 * 100), (int)(num9 * 100));
        }

        public Color ToColor()
        {
            double num4 = 0;
            double num5 = 0;
            double num6 = 0;
            double num = ((double) this.Hue) % 360;
            double num2 = ((double) this.Saturation) / 100;
            double num3 = ((double) this.Value) / 100;
            if (num2 == 0)
            {
                num4 = num3;
                num5 = num3;
                num6 = num3;
            }
            else
            {
                double d = num / 60;
                int num11 = (int) Math.Floor(d);
                double num10 = d - num11;
                double num7 = num3 * (1 - num2);
                double num8 = num3 * (1 - (num2 * num10));
                double num9 = num3 * (1 - (num2 * (1 - num10)));
                switch (num11)
                {
                    case 0:
                        num4 = num3;
                        num5 = num9;
                        num6 = num7;
                        goto Label_0136;

                    case 1:
                        num4 = num8;
                        num5 = num3;
                        num6 = num7;
                        goto Label_0136;

                    case 2:
                        num4 = num7;
                        num5 = num3;
                        num6 = num9;
                        goto Label_0136;

                    case 3:
                        num4 = num7;
                        num5 = num8;
                        num6 = num3;
                        goto Label_0136;

                    case 4:
                        num4 = num9;
                        num5 = num7;
                        num6 = num3;
                        goto Label_0136;

                    case 5:
                        num4 = num3;
                        num5 = num7;
                        num6 = num8;
                        goto Label_0136;
                }
            }
        Label_0136:
            return Color.FromArgb((int)(num4 * 255), (int)(num5 * 255), (int)(num6 * 255));
        }
    }
}
