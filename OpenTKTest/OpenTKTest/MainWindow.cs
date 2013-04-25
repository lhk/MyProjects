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
using OpenTKTest.RenderTools;
using System.Drawing.Imaging;

namespace OpenTKTest
{
    class MainWindow : GameWindow
    {
        public MainWindow() : base(DisplayDevice.Default.Width, DisplayDevice.Default.Height, Graphics.GraphicsMode.Default, "Title", GameWindowFlags.Fullscreen, DisplayDevice.Default){}

        VertexBufferObject vboQuad;
        VertexBufferObject vboSphere;
        Texture TextureDots;
        Texture TextureGreen;
        Camera cam = new Camera();

        Point lastCursorPoint = System.Windows.Forms.Cursor.Position;

        protected override void OnLoad(EventArgs e)
        {
            System.Windows.Forms.Cursor.Hide();

            GL.ClearColor(Color.Black);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);


            vboSphere = BasicShapes.Sphere(16);
            vboQuad = BasicShapes.Quad(16);

            TextureDots = new Texture("dots.png");
            TextureGreen = new Texture(Color.LightGreen);

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


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
            {
                Exit();
                System.Windows.Forms.Cursor.Show();
            }

            // camera movement
            Vector3 v = Vector3.Zero;

            if (Keyboard[Key.A]) v.X--;
            if (Keyboard[Key.D]) v.X++;

            if (Keyboard[Key.W]) v.Z--;
            if (Keyboard[Key.S]) v.Z++;

            if (Keyboard[Key.Q]) v.Y--;
            if (Keyboard[Key.E]) v.Y++;

            cam.Move(v);

            // cursor movement
            int dx = lastCursorPoint.X - System.Windows.Forms.Cursor.Position.X;
            int dy = lastCursorPoint.Y - System.Windows.Forms.Cursor.Position.Y;
            lastCursorPoint = System.Windows.Forms.Cursor.Position = new Point(Width/2,Height/2);

            // camera rotation
            cam.Yaw -= dx;
            cam.Pitch -= dy;

            base.OnUpdateFrame(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 ViewMatrix = cam.ViewMatrix;
            GL.MatrixMode(MatrixMode.Modelview);

            
            

            //light
            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { .5f, .5f, .5f, 0 });

            GL.Enable(EnableCap.Light1);
            GL.Light(LightName.Light1, LightParameter.Diffuse, new float[] { 300, 300, 300, 0 });
            GL.Light(LightName.Light1, LightParameter.Position, new float[] { 1, 3, 1, 0 });
            
            
            // matrix chain
            GL.LoadIdentity();
            GL.MultMatrix(ref ViewMatrix);
            GL.Translate(0, 0, -30);
            GL.Rotate(90, 1, 0, 1);
            GL.Scale(20, 20, 20);
            // texture
            TextureGreen.Bind();
            // just do it!
            vboSphere.Render();


            


            // matrix chain
            GL.LoadIdentity();
            GL.MultMatrix(ref ViewMatrix);
            GL.Translate(0, -50, -30);
            GL.Scale(200, 200, 200);
            // texture
            TextureDots.Bind();
            // just do it!
            vboQuad.Render();



            SwapBuffers();
            base.OnRenderFrame(e);
        }
    }

}
