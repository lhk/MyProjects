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
using CurveFever3DTest.RenderTools;
using System.Drawing.Imaging;
using OpenTK.Platform;
using System.Reflection;
using System.Net.Sockets;

namespace CurveFever3DTest
{
    class MainWindow : GameWindow
    {
        public MainWindow() : base(DisplayDevice.Default.Width, DisplayDevice.Default.Height, Graphics.GraphicsMode.Default, "Title", GameWindowFlags.Fullscreen, DisplayDevice.Default){}
        
        IntPtr windowHandle = IntPtr.Zero;

        VertexBufferObject vboQuad;
        VertexBufferObject vboQuadSingleTexture;
        VertexBufferObject vboSphere;
        VertexBufferObject vboTube;

        Texture TextureDots;
        Texture TextureSkull;
        Texture TextureRed;
        Texture TextureHelloWorld;

        GameLogic gameLogic = new GameLogic();
        bool gameover = false;

        Camera cam = new Camera();

        Point lastCursorPoint = System.Windows.Forms.Cursor.Position;
        DateTime lastPathLog = DateTime.Now;

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

            vboSphere = BasicShapes.Sphere(20);
            vboQuad = BasicShapes.Quad(16); 
            vboQuadSingleTexture = BasicShapes.Quad(1);
            vboTube = BasicShapes.Tube(16);

            TextureDots = new Texture("dots.png");
            TextureSkull = new Texture("skull.png");
            TextureRed = new Texture(Color.Red);
            TextureHelloWorld = new Texture("Hello World", new Font("Arial", 90), Color.Green);

            cam.Position = new Vector3(40,40,40);

            base.OnLoad(e);
        }        

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), (float) Width / Height, 1e-3f, 1e6f);
            GL.MultMatrix(ref perspective);


            base.OnResize(e);
        }




        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // camera setup
            Matrix4 ViewMatrix = cam.ViewMatrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MultMatrix(ref ViewMatrix);

            if (windowHandle == IntPtr.Zero) windowHandle = GetForegroundWindow();

            if (windowHandle == GetForegroundWindow())
            {

                if (Keyboard[Key.Escape])
                {
                    Exit();
                    System.Windows.Forms.Cursor.Show();
                }

                if (Keyboard[Key.P]) gameover = false;

                cam.Position += cam.Direction * 2e-1f;

                // cursor movement
                int dx = lastCursorPoint.X - System.Windows.Forms.Cursor.Position.X;
                int dy = lastCursorPoint.Y - System.Windows.Forms.Cursor.Position.Y;
                lastCursorPoint = System.Windows.Forms.Cursor.Position = new Point(Width / 2, Height / 2);

                // camera rotation
                cam.Yaw -= dx * cam.RotationSensitivity;
                cam.Pitch -= dy * cam.RotationSensitivity;
                cam.Yaw %= MathHelper.TwoPi;



                // pathlog
                if (!gameLogic.AddPoint(cam.Position))
                {
                    gameover = true;
                }
            }

            drawPath();
            drawBox();
            drawHUD();

            SwapBuffers();
            base.OnRenderFrame(e);
        }

        private void drawPath()
        {
            for (int i = 0; i < gameLogic.path.Count - 2; i++)
            {
                Vector3 diff = gameLogic.path[i + 1] - gameLogic.path[i];
                float len = diff.LengthFast;
                float a;
                Vector3 dummy = Vector3.UnitY;
                Vector3.CalculateAngle(ref diff, ref dummy, out a);

                GL.PushMatrix();
                GL.Translate(gameLogic.path[i]);
                GL.Scale(1, -1, 1);
                GL.Rotate(MathHelper.RadiansToDegrees(a) + 180, Vector3.Cross(diff, Vector3.UnitY));
                GL.Scale(1, len, 1);
                TextureDots.Bind();
                vboTube.Render();
                GL.PopMatrix();
            }
        }

        private void drawHUD()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Modelview);
            
            GL.LoadIdentity();
            GL.Translate(0, 0, -100);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(10,1,1);
            GL.Translate(-.5f, 0, -.5f);
            TextureRed.Bind();
            vboQuad.Render();

            GL.LoadIdentity();
            GL.Translate(0, 0, -100);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(1, 1, 10);
            GL.Translate(-.5f, 0, -.5f);
            TextureRed.Bind();
            vboQuad.Render();

            GL.LoadIdentity();
            GL.Translate(0, 0, -1000);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(TextureHelloWorld.Size.Width, 1, TextureHelloWorld.Size.Height);
            GL.Translate(-.5f, 0, -.5f);
            TextureHelloWorld.Bind();
            vboQuadSingleTexture.Render();


            GL.Enable(EnableCap.DepthTest);
        }

        private void drawBox()
        {
            (gameover ? TextureRed : TextureSkull).Bind();

            // bottom
            GL.PushMatrix();
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            // top
            GL.PushMatrix();
            GL.Translate(0, gameLogic.boundingCubeSize, 0);
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            // 4 sides
            GL.PushMatrix();
            GL.Translate(0, gameLogic.boundingCubeSize, 0);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(0, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(0, gameLogic.boundingCubeSize, 0);
            GL.Rotate(-90, Vector3.UnitY);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, 0);
            GL.Rotate(-90, Vector3.UnitY);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameLogic.boundingCubeSize, gameLogic.boundingCubeSize, gameLogic.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

        }
    }

}
