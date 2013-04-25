using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace CurveFever3DTest.RenderTools
{
    public class Camera
    {
        public Vector3 Position;
        public float Yaw; // axis: world-Y
        public float Pitch; // axis: eye-X
        public float RotationSensitivity = 0.001f;
        public float MovementSpeed = 0.5f;

        public void Move(Vector3 v)
        {
            v *= MovementSpeed;
            Position.Y += v.Y;
            Position.X += (float)(Math.Cos(Yaw) * v.X - Math.Sin(Yaw) * v.Z);
            Position.Z += (float)(Math.Sin(Yaw) * v.X + Math.Cos(Yaw) * v.Z);
        }

        public Vector3 Direction
        {
            get
            {
                Vector3 d = Vector3.Zero;
                d.X = (float)(Math.Sin(Yaw) * Math.Cos(Pitch));
                d.Z = (float)(-Math.Cos(Yaw) * Math.Cos(Pitch));
                d.Y = (float)(-Math.Sin(Pitch));
                return d;
            }
        }

        public Matrix4 ViewMatrix
        {
            get
            {
                if (Pitch > MathHelper.PiOver2 - 0.0001f) Pitch = MathHelper.PiOver2 - 0.0001f;
                if (Pitch < -(MathHelper.PiOver2 - 0.0001f)) Pitch = -(MathHelper.PiOver2 - 0.0001f);

                return Matrix4.CreateTranslation(-Position)
                    * Matrix4.CreateRotationY(Yaw)
                    * Matrix4.CreateRotationX(Pitch);
            }
        }
    }
}
