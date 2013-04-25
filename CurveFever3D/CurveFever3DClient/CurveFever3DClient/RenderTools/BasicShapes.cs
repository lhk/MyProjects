using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace CurveFever3DClient.RenderTools
{
    public static class BasicShapes
    {
        public static VertexBufferObject Sphere(int LevelOfDetail)
        {
            if (LevelOfDetail < 4) LevelOfDetail = 4;
            if (LevelOfDetail > 50) LevelOfDetail = 50;


            ushort[] indexArray = new ushort[(LevelOfDetail - 1) * (LevelOfDetail - 1) * 2 * 3];
            for (int x = 0; x < LevelOfDetail - 1; x++)
            {
                for (int y = 0; y < LevelOfDetail - 1; y++)
                {
                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 0] = (ushort)(y * LevelOfDetail + x);
                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 1] = (ushort)(y * LevelOfDetail + (x + 1));
                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 2] = (ushort)((y + 1) * LevelOfDetail + x);

                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 3] = (ushort)((y + 1) * LevelOfDetail + x);
                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 4] = (ushort)(y * LevelOfDetail + (x + 1));
                    indexArray[(y * (LevelOfDetail - 1) + x) * 6 + 5] = (ushort)((y + 1) * LevelOfDetail + (x + 1));
                }
            }


            Vertex[] vertexArray = new Vertex[LevelOfDetail * LevelOfDetail];

            for (int x = 0; x < LevelOfDetail; x++)
            {
                for (int y = 0; y < LevelOfDetail; y++)
                {
                    vertexArray[y * LevelOfDetail + x] =
                        new Vertex(
                            new Vector3((float)x / (LevelOfDetail - 1), (float)y / (LevelOfDetail - 1), 0),
                            Vector3.UnitY,
                            new Vector2((float)x / (LevelOfDetail - 1), (float)y / (LevelOfDetail - 1))
                        );

                    float a = vertexArray[y * LevelOfDetail + x].Position.X * MathHelper.TwoPi;
                    float a_y = (vertexArray[y * LevelOfDetail + x].Position.Y - 0.5f) * MathHelper.Pi;
                    vertexArray[y * LevelOfDetail + x].Position.X = (float)(Math.Cos(a));
                    vertexArray[y * LevelOfDetail + x].Position.Z = (float)(Math.Sin(a));
                    vertexArray[y * LevelOfDetail + x].Position.Y = (float)(Math.Sin(a_y));
                    float _y = vertexArray[y * LevelOfDetail + x].Position.Y;
                    vertexArray[y * LevelOfDetail + x].Position.X *= (float)(Math.Sqrt(1 - _y * _y));
                    vertexArray[y * LevelOfDetail + x].Position.Z *= (float)(Math.Sqrt(1 - _y * _y));

                    vertexArray[y * LevelOfDetail + x].Normal = vertexArray[y * LevelOfDetail + x].Position;
                    vertexArray[y * LevelOfDetail + x].Normal.Y = 0;
                    vertexArray[y * LevelOfDetail + x].Normal.Normalize();
                }
            }

            VertexBufferObject v = new VertexBufferObject(vertexArray, indexArray);
            v.Load();
            return v;
        }

        public static VertexBufferObject Tube(int LevelOfDetail)
        {
            if (LevelOfDetail < 4) LevelOfDetail = 4;
            if (LevelOfDetail > 50) LevelOfDetail = 50;
            
            ushort[] indexArray = new ushort[(LevelOfDetail - 1) * 2 * 3];
            
            for (int x = 0; x < LevelOfDetail-1; x++)
            {
                indexArray[x * 6 + 0] = (ushort)(x);
                indexArray[x * 6 + 1] = (ushort)( (x + 1));
                indexArray[x * 6 + 2] = (ushort)(LevelOfDetail + x);

                indexArray[x * 6 + 3] = (ushort)(LevelOfDetail + x);
                indexArray[x * 6 + 4] = (ushort)((x + 1));
                indexArray[x * 6 + 5] = (ushort)(LevelOfDetail + (x + 1));
                
            }


            Vertex[] vertexArray = new Vertex[2 * LevelOfDetail];

            for (int x = 0; x < LevelOfDetail; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    vertexArray[y * LevelOfDetail + x] =
                        new Vertex(
                            new Vector3((float)x / (LevelOfDetail - 1), (float)y, 0),
                            Vector3.UnitY,
                            new Vector2((float)x / (LevelOfDetail - 1), (float)y / (LevelOfDetail - 1))
                        );

                    float a = vertexArray[y * LevelOfDetail + x].Position.X * MathHelper.TwoPi;
                    float a_y = (vertexArray[y * LevelOfDetail + x].Position.Y - 0.5f) * MathHelper.Pi;
                    vertexArray[y * LevelOfDetail + x].Position.X = (float)(Math.Cos(a));
                    vertexArray[y * LevelOfDetail + x].Position.Z = (float)(Math.Sin(a));

                    vertexArray[y * LevelOfDetail + x].Normal = vertexArray[y * LevelOfDetail + x].Position;
                    vertexArray[y * LevelOfDetail + x].Normal.Normalize();
                }
            }

            VertexBufferObject v = new VertexBufferObject(vertexArray, indexArray);
            v.Load();
            return v;
        }

        public static VertexBufferObject Quad(float textureScale)
        {
            ushort[] indexArray = new ushort[]
            {
                0,1,2,  
                2,1,3,   
            };

            Vertex[] vertexArray = new Vertex[]
            {
                new Vertex(new Vector3(0,0,0),Vector3.UnitY,new Vector2(0,0)),
                new Vertex(new Vector3(1,0,0),Vector3.UnitY,new Vector2(textureScale,0)),
                new Vertex(new Vector3(0,0,1),Vector3.UnitY,new Vector2(0,textureScale)),
                new Vertex(new Vector3(1,0,1),Vector3.UnitY,new Vector2(textureScale,textureScale)),
            };

            VertexBufferObject v = new VertexBufferObject(vertexArray, indexArray);
            v.Load();
            return v;
        }
    }
}
