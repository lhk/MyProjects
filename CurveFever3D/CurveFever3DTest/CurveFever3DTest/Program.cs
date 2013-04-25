using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CurveFever3DTest
{
    static class Program
    {
        static void Main()
        {
            MainWindow w = new MainWindow();
            w.WindowState = OpenTK.WindowState.Fullscreen;
            w.Run();
        }
    }
}
