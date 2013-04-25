using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Graphics = OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace CurveFever3DClient.RenderTools
{
    static class HUD
    {
        public static void drawMessage(float x, float y, string msg)
        {
            Texture t = ObjectCache.Get("Texture(" + msg + ")", () => new Texture(msg, new Font("Arial", 20), Color.White));

            GL.LoadIdentity();
            GL.Translate(x, y, 0);
            GL.Rotate(90, Vector3.UnitX);
            GL.Scale(t.Size.Height * 0.0011f, 1, t.Size.Width * 0.0011f);
            GL.Translate(-.5f, 0, -.5f);
            t.Bind();
            ObjectCache.Get("BasicShapes.Quad(1)", () => BasicShapes.Quad(1)).Render();
        }

    }
}
