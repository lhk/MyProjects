using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTKTest.RenderTools
{
    public class Camera
    {
        Vector3 Position;
        public float Yaw; // axis: world-Y
        public float Pitch; // axis: eye-X
        public float RotationSensitivity = 0.001f;
        public float MovementSpeed = 0.5f;

        public void Move(Vector3 v)
        {
            v *= MovementSpeed;
            Position.Y += v.Y;
            Position.X += (float)(Math.Cos(Yaw * RotationSensitivity) * v.X - Math.Sin(Yaw * RotationSensitivity) * v.Z);
            Position.Z += (float)(Math.Sin(Yaw * RotationSensitivity) * v.X + Math.Cos(Yaw * RotationSensitivity) * v.Z);
        }

        public Matrix4 ViewMatrix
        {
            get
            {
                if (Pitch * RotationSensitivity > MathHelper.PiOver2 - 0.0001f) Pitch = (MathHelper.PiOver2 - 0.0001f) / RotationSensitivity;
                if (Pitch * RotationSensitivity < -(MathHelper.PiOver2 - 0.0001f)) Pitch = -(MathHelper.PiOver2 - 0.0001f) / RotationSensitivity;

                return Matrix4.CreateTranslation(-Position)
                    * Matrix4.CreateRotationY(Yaw * RotationSensitivity)
                    * Matrix4.CreateRotationX(Pitch * RotationSensitivity);
            }
        }
    }
}
