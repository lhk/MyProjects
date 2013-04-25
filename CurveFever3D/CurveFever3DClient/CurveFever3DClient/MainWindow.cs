using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Graphics = OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;
using System.Runtime.InteropServices;
using CurveFever3DClient.RenderTools;
using System.Drawing.Imaging;
using OpenTK.Platform;
using System.Reflection;
using System.Net.Sockets;

namespace CurveFever3DClient
{
    class MainWindow : GameWindow
    {
        public MainWindow() : base(DisplayDevice.Default.Width, DisplayDevice.Default.Height, Graphics.GraphicsMode.Default, "Title", GameWindowFlags.Fullscreen, DisplayDevice.Default){}
        
        IntPtr windowHandle = IntPtr.Zero;
        Camera cam = new Camera();
        Point lastCursorPoint = System.Windows.Forms.Cursor.Position;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        protected override void OnLoad(EventArgs e)
        {
            System.Windows.Forms.Cursor.Hide();

            GL.ClearColor(Color.Black);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //light
            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 5, 5, 5, 0 });
            
            cam.Position = new Vector3(40,40,40);

            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), (float)Width / Height, 1e-3f, 1e6f);
            GL.MultMatrix(ref perspective);

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (windowHandle == IntPtr.Zero) windowHandle = GetForegroundWindow();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // camera setup
            Matrix4 ViewMatrix = cam.ViewMatrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MultMatrix(ref ViewMatrix);

            

            if (windowHandle == GetForegroundWindow())
            {
                if (Keyboard[Key.Escape])
                {
                    System.Windows.Forms.Cursor.Show();
                    Exit();
                }
                // cursor movement
                int dx = lastCursorPoint.X - System.Windows.Forms.Cursor.Position.X;
                int dy = lastCursorPoint.Y - System.Windows.Forms.Cursor.Position.Y;
                lastCursorPoint = System.Windows.Forms.Cursor.Position = new Point(Width / 2, Height / 2);

                // camera rotation
                cam.Yaw -= dx * cam.RotationSensitivity;
                cam.Pitch -= dy * cam.RotationSensitivity;
                cam.Yaw %= MathHelper.TwoPi;
            }

            GL.PushMatrix();
            GL.Translate(0,0,-50);
            GL.Scale(10,10,10);

            ObjectCache.Get("Texture(Color.Red)", () => new Texture(Color.Red)).Bind();
            ObjectCache.Get("BasicShapes.Sphere(20)", () => BasicShapes.Sphere(20)).Render();
            GL.PopMatrix();

            drawHUD();

            SwapBuffers();
            base.OnRenderFrame(e);
        }

        private void drawHUD()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Disable(EnableCap.DepthTest);



            Texture.TextureFilterLinear();
            HUD.drawMessage(0,0,"Lorem ipsum dolor sit amet, consectetuer adipiscing elit.\n Aenean commodo ligula eget dolor. Aenean massa. Cum sociis\n natoque penatibus et magnis dis parturient m\nontes, nascetur ridiculus mus\n. Donec quam felis, ultricies nec\n, pellentesque eu, pretium quis, \nsem. Nulla consequat massa quis \nenim. Donec pede justo\n, fringilla vel, aliquet \nnec, vulputate eget, arcu\n. In enim justo, \nrhoncus ut, imperdiet a, \nvenenatis vitae, justo. Nullam \ndictum felis eu pede mollis pretium. \nInteger tincidunt. Cras dapibus. \nVivamus elementum semper nisi. Aenean vulputate \neleifend tellus. Aenean leo \nligula, porttitor eu\n, consequat vitae, eleifend ac.");
            //HUD.drawMessage(-1, 1, "0");

            GL.Enable(EnableCap.DepthTest);



            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), (float)Width / Height, 1e-3f, 1e6f);
            GL.MultMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}
