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

namespace MinesweeperBot
{
    public partial class MinesweeperInterface : Form
    {
        Bitmap screenshot;
        byte[, ,] defaultImageByteArray;
        byte[, ,] screenshotByteArray;
        int[, ,] differencesArray;
        char[,] Categorization;
        /// <summary>
        /// next click point
        /// special states:
        /// (-1,-1) nothing found
        /// (-2,-2) no search occured, no data availbe
        /// </summary>
        Point nextClick = new Point(-2,-2);
        Point lastClick = new Point(-2, -2);

        public MinesweeperInterface()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            defaultImageByteArray = BitmapToByteArray((Bitmap)Bitmap.FromFile(Program.GetBasePath()+"\\defaultImage.png"));
        }

        private void Scan()
        {
            if (GetTopWindowText() == "Minesweeper")
            {
                screenshot = CaptureScreenshot();
                //screenshot.Save(Program.GetBasePath() + "\\defaultImage.png");
                screenshotByteArray = BitmapToByteArray(screenshot);

                if (screenshotByteArray.Length == defaultImageByteArray.Length)
                {
                    differencesArray = Differences(screenshotByteArray, defaultImageByteArray);
                    Categorization = CategorizeField();

                    if (GetKeyState(0x2E) < 0)
                    {
                        nextClick = GameSolver.FindNextClick(Categorization, lastClick);

                        if (nextClick.X >= 0 && GetTopWindowText() == "Minesweeper")
                        {
                            RECT rect;
                            if (GetWindowRect(new HandleRef(this, GetForegroundWindow()), out rect))
                            {
                                Rectangle bounds = new Rectangle(rect.Left + 39, rect.Top + 81, rect.Width - 39 - 37, rect.Height - 81 - 40);
                                SetCursorPos(bounds.X + nextClick.X * 18 + 5, bounds.Y + nextClick.Y * 18 + 5);
                                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                                    (uint)(bounds.X + nextClick.X * 18 + 5),
                                    (uint)(bounds.Y + nextClick.Y * 18 + 5), 0, 0);
                            }
                            lastClick = nextClick;
                        }
                    }
                }
                else
                {
                    screenshot = null;
                    nextClick = new Point(-2, -2);
                }
            }
            else
            {
                screenshot = null;
                nextClick = new Point(-2, -2);
            }
        }

        private void MinesweeperInterface_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                GC.Collect();
                Scan();
                GC.Collect();

                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TranslateTransform(20, 20);
                Text = "MinesweeperInterface - #data: " + Storage.s.DataSet.Count.ToString();
                if (screenshot != null)
                {
                    e.Graphics.DrawImage(screenshot, 20, 20);

                    for (int x = 0; x < 30; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            if (Categorization[x, y] == 'x')
                                g.FillRectangle(new SolidBrush(Color.Blue), 18 * x + 25 - 5, 18 * y + 350 - 2, 18, 18);
                            else if (Categorization[x, y] == 'f')
                            {
                                g.FillRectangle(new SolidBrush(Color.Blue), 18 * x + 25 - 5, 18 * y + 350 - 2, 18, 18);
                                g.FillPolygon(new SolidBrush(Color.Red), new Point[] {
                                new Point( 18 * x + 25 - 5+3, 18 * y + 350 - 2+3),
                                new Point( 18 * x + 25 - 5+3, 18 * y + 350 - 2+15),
                                new Point( 18 * x + 25 - 5+15, 18 * y + 350 - 2+7)
                                });
                            }
                            else if (Categorization[x, y] != '0')
                                g.DrawString(Categorization[x, y].ToString(), new Font("Arial", 9), new SolidBrush(Color.Black), 18 * x + 25, 18 * y + 350);
                        }
                    }
                    if (nextClick.X >= 0)
                    {
                        int radius = 8;
                        g.DrawEllipse(new Pen(Color.Red, 3), 18 * nextClick.X + 46 - 25, 18 * nextClick.Y + 350, radius * 2, radius * 2);
                    }
                    else if (nextClick.X == -1)
                    {
                        g.DrawString("No Click-Point found.", new Font("Arial", 20), new SolidBrush(Color.Red), 50, 750);
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    g.DrawLine(new Pen(Color.Red, 3), 10, 10, 100, 100);
                    g.DrawLine(new Pen(Color.Red, 3), 10, 100, 100, 10);
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
            Invalidate();
        }

        char[,] CategorizeField()
        {
            char[,] result = new char[30, 16];
            bool allowDataAdding = false;

            for (int x = 0; x < result.GetUpperBound(0) + 1; x++)
                for (int y = 0; y < result.GetUpperBound(1) + 1; y++)
                    result[x, y] = CategorizeBlock(18 * x + 1, 18 * y + 1, 16, 16, ref allowDataAdding);

            return result;
        }

        char CategorizeBlock(int offset_x, int offset_y, int width, int height, ref bool allowDataAdding)
        {
            // check for unknown field
            {
                bool allDiffZero = true;
                for (int x = 0; x < width && allDiffZero; x++)
                    for (int y = 0; y < height && allDiffZero; y++)
                        for (int i = 0; i < 3 && allDiffZero; i++)
                            if (differencesArray[offset_x + x, offset_y + y, i] != 0) 
                                allDiffZero = false;

                if (allDiffZero)
                {
                    allowDataAdding = true;
                    return 'x'; 
                }
            }
            
            // create new data point for learning algorithm
            DataPoint p = new DataPoint();
            p.Features = new double[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    p.Features[x * width + y] =
                        (screenshotByteArray[offset_x + x, offset_y + y, 0] +
                        screenshotByteArray[offset_x + x, offset_y + y, 1] +
                        screenshotByteArray[offset_x + x, offset_y + y, 2]);
                }
            }
            p.Label = SupervisedLearningAlgo.evaluateFunction(p);

            // add datapoint to database
            if (allowDataAdding && !Storage.s.DataSet.Contains(p))
            {
                Storage.s.DataSet.Add(p);
            }


            // evaluate classifier function
            return p.Label;
        }

        private byte[, ,] BitmapToByteArray(Bitmap bitmap)
        {
            byte[,,] byteArray = new byte[bitmap.Width,bitmap.Height,3];
            bmp.FastBitmap fbmp = new bmp.FastBitmap(bitmap);

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color c = fbmp.GetPixel(x,y);
                    byteArray[x, y, 0] = c.R;
                    byteArray[x, y, 1] = c.G;
                    byteArray[x, y, 2] = c.B;
                }
            }
            return byteArray;
        }

        private Bitmap ByteArrayToBitmap(byte[, ,] byteArray)
        {
            if (byteArray.GetUpperBound(2) + 1 != 3) throw new Exception("Invalid input");

            bmp.FastBitmap fbmp = new bmp.FastBitmap(byteArray.GetUpperBound(0) + 1, byteArray.GetUpperBound(1) + 1);

            for (int x = 0; x < fbmp.Width; x++)
            {
                for (int y = 0; y < fbmp.Height; y++)
                {
                    fbmp.SetPixel(x, y, Color.FromArgb(byteArray[x, y, 0], byteArray[x, y, 1], byteArray[x, y, 2]));
                }
            }
            return fbmp.ToBitmap();
        }

        private Bitmap CaptureScreenshot()
        {
            RECT rect;
            Bitmap bitmap = null;
            if (GetWindowRect(new HandleRef(this, GetForegroundWindow()), out rect))
            {
                Rectangle bounds = new Rectangle(rect.Left + 39, rect.Top + 81, rect.Width - 39 - 37, rect.Height - 81 - 40);
                bitmap = new Bitmap(bounds.Width, bounds.Height);
                using (Graphics g = Graphics.FromImage(bitmap)) g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        private int[, ,] Differences(byte[, ,] a, byte[, ,] b)
        {
            int[, ,] res = new int[a.GetUpperBound(0) + 1, a.GetUpperBound(1) + 1, a.GetUpperBound(2) + 1];

            for (int x = 0; x < a.GetUpperBound(0) + 1 && x < b.GetUpperBound(0) + 1; x++)
                for (int y = 0; y < a.GetUpperBound(1) + 1 && y < b.GetUpperBound(1) + 1; y++)
                    for (int z = 0; z < a.GetUpperBound(2) + 1 && z < b.GetUpperBound(2) + 1; z++)
                        res[x, y, z] = a[x, y, z] - b[x, y, z];

            return res;
        }

        public static string GetTopWindowText()
        {
            IntPtr hWnd = GetForegroundWindow();
            int length = GetWindowTextLength(hWnd);
            StringBuilder text = new StringBuilder(length + 1);
            GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }


        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern short GetKeyState(int virtualKeyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private void MinesweeperInterface_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle && screenshot!=null)
            {
                screenshot.Save(Program.GetBasePath() + "\\defaultImage.png");
            }
        }
    }
}
