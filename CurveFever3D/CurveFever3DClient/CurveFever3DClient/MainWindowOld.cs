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
    class MainWindowOld : GameWindow
    {
        public MainWindowOld() : base(DisplayDevice.Default.Width, DisplayDevice.Default.Height, Graphics.GraphicsMode.Default, "Title", GameWindowFlags.Fullscreen, DisplayDevice.Default) { }

        Socket clientSocket;

        IntPtr windowHandle = IntPtr.Zero;

        VertexBufferObject vboQuad;
        VertexBufferObject vboSphere;
        VertexBufferObject vboTube;

        Texture TextureDots;
        Texture TextureSkull;
        Texture TextureRed;

        GameData gameData = new GameData();

        Camera cam = new Camera();

        Point lastCursorPoint = System.Windows.Forms.Cursor.Position;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        protected override void OnLoad(EventArgs e)
        {
            connect();

            System.Windows.Forms.Cursor.Hide();

            GL.ClearColor(Color.Black);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //light
            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 5, 5, 5, 0 });

            vboSphere = BasicShapes.Sphere(20);
            vboQuad = BasicShapes.Quad(16);
            vboTube = BasicShapes.Tube(16);

            TextureDots = new Texture("dots.png");
            TextureSkull = new Texture("skull.png");
            TextureRed = new Texture(Color.Red);

            cam.Position = new Vector3(40, 40, 40);

            base.OnLoad(e);
        }

        private void connect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.NoDelay = true;
            clientSocket.Connect("178.63.14.80", 3449);
        }



        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), (float)Width / Height, 1e-3f, 1e6f);
            GL.MultMatrix(ref perspective);


            base.OnResize(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (windowHandle == IntPtr.Zero) windowHandle = GetForegroundWindow();

            if (windowHandle == GetForegroundWindow())
            {

                if (Keyboard[Key.Escape])
                {
                    Exit();
                    System.Windows.Forms.Cursor.Show();
                }

                //if (Keyboard[Key.P]) gameover = false;

                // camera movement
                /*
                Vector3 v = Vector3.Zero;

                if (Keyboard[Key.A]) v.X--;
                if (Keyboard[Key.D]) v.X++;

                if (Keyboard[Key.W]) v.Z--;
                if (Keyboard[Key.S]) v.Z++;

                if (Keyboard[Key.Q]) v.Y--;
                if (Keyboard[Key.E]) v.Y++;

                cam.Move(v);*/

                //cam.Position += cam.Direction * 2e-1f;


                // cursor movement
                int dx = lastCursorPoint.X - System.Windows.Forms.Cursor.Position.X;
                int dy = lastCursorPoint.Y - System.Windows.Forms.Cursor.Position.Y;
                lastCursorPoint = System.Windows.Forms.Cursor.Position = new Point(Width / 2, Height / 2);

                // camera rotation
                cam.Yaw -= dx * cam.RotationSensitivity;
                cam.Pitch -= dy * cam.RotationSensitivity;
                cam.Yaw %= MathHelper.TwoPi;



            }





            // network test
            if (clientSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[4096];
                int bytes = clientSocket.Receive(buffer);
                string[] lines = Encoding.ASCII.GetString(buffer, 0, bytes).Split('\n');

                foreach (var line in lines)
                {
                    string[] elements = line.Trim().Split('=');
                    if (0 < elements.Length && elements.Length <= 2)
                    {
                        string name = elements[0].Trim();

                        if (name == "YourPosition")
                        {
                            string[] coordinates = elements[1].Substring(1, elements[1].Length - 2).Split(':');
                            float x, y, z;
                            if (coordinates.Length == 3 &&
                                float.TryParse(coordinates[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                                float.TryParse(coordinates[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                                float.TryParse(coordinates[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
                            {
                                cam.Position = new Vector3(x, y, z);
                            }
                        }
                    }
                }

                //Console.WriteLine("Received: \"" + Encoding.ASCII.GetString(buffer, 0, bytes) + "\"");
            }
            if (clientSocket.Poll(0, SelectMode.SelectWrite))
            {
                string s = "Pitch=" + (cam.Pitch).ToString("############0.0#############", System.Globalization.CultureInfo.InvariantCulture) + "\n"
                    + "Yaw=" + (cam.Yaw).ToString("############0.0#############", System.Globalization.CultureInfo.InvariantCulture) + "\n";
                byte[] sendBuffer = Encoding.ASCII.GetBytes(s);
                if (sendBuffer.Length > 0)
                    clientSocket.Send(sendBuffer);
            }


            base.OnUpdateFrame(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // camera setup
            Matrix4 ViewMatrix = cam.ViewMatrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MultMatrix(ref ViewMatrix);

            drawPath();
            drawBox();
            drawHUD();

            SwapBuffers();
            base.OnRenderFrame(e);
        }

        private void drawPath()
        {
            for (int i = 0; i < gameData.path.Count - 2; i++)
            {
                Vector3 diff = gameData.path[i + 1] - gameData.path[i];
                float len = diff.LengthFast;
                float a;
                Vector3 dummy = Vector3.UnitY;
                Vector3.CalculateAngle(ref diff, ref dummy, out a);

                GL.PushMatrix();
                GL.Translate(gameData.path[i]);
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
            GL.Scale(10, 1, 1);
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


            GL.Enable(EnableCap.DepthTest);
        }

        private void drawBox()
        {
            TextureSkull.Bind();

            // bottom
            GL.PushMatrix();
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            // top
            GL.PushMatrix();
            GL.Translate(0, gameData.boundingCubeSize, 0);
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            // 4 sides
            GL.PushMatrix();
            GL.Translate(0, gameData.boundingCubeSize, 0);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(0, gameData.boundingCubeSize, gameData.boundingCubeSize);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(0, gameData.boundingCubeSize, 0);
            GL.Rotate(-90, Vector3.UnitY);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(gameData.boundingCubeSize, gameData.boundingCubeSize, 0);
            GL.Rotate(-90, Vector3.UnitY);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(gameData.boundingCubeSize, gameData.boundingCubeSize, gameData.boundingCubeSize);
            vboQuad.Render();
            GL.PopMatrix();

        }
    }

}
