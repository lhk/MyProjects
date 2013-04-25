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

namespace OpenTKTest.RenderTools
{
    public class SimpleVertexBufferObject
    {
        uint VertexBufferHandle = 0;
        uint IndexBufferHandle = 0;

        public float[] VertexBuffer;
        public ushort[] IndexBuffer;

        public SimpleVertexBufferObject(float[] _vertexBuffer, ushort[] _indexBuffer)
        {
            VertexBuffer = _vertexBuffer;
            IndexBuffer = _indexBuffer;
        }

        public void Load()
        {
            // Create handles
            GL.GenBuffers(1, out VertexBufferHandle);
            GL.GenBuffers(1, out IndexBufferHandle);

            // Fill in the data
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexBuffer.Length *  sizeof(float)), VertexBuffer, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(IndexBuffer.Length * sizeof(ushort)), IndexBuffer, BufferUsageHint.StaticDraw);
        }

        public void Render()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.DrawElements(BeginMode.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedShort, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
        }
    }

    public class VertexBufferObject
    {
        uint VertexBufferHandle = 0;
        uint IndexBufferHandle = 0;

        public Vertex[] VertexBuffer;
        public ushort[] IndexBuffer;

        public VertexBufferObject(Vertex[] _vertexBuffer, ushort[] _indexBuffer)
        {
            VertexBuffer = _vertexBuffer;
            IndexBuffer = _indexBuffer;
        }

        public void Load()
        {
            // Create handles
            GL.GenBuffers(1, out VertexBufferHandle);
            GL.GenBuffers(1, out IndexBufferHandle);

            // Fill in the data
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexBuffer.Length * 32), VertexBuffer, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(IndexBuffer.Length * sizeof(ushort)), IndexBuffer, BufferUsageHint.StaticDraw);
        }

        public void Render()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            
            GL.VertexPointer(3, VertexPointerType.Float, Vertex.Stride, 0);
            GL.NormalPointer(NormalPointerType.Float, Vertex.Stride, 12);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Stride, 24);

            GL.DrawElements(BeginMode.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedShort, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position; // bytes 0 - 11
        public Vector3 Normal; // bytes 12 - 23
        public Vector2 TexCoord; // bytes 24 - 31

        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
        public Vertex(Vector3 _Position, Vector3 _Normal, Vector2 _TexCoord)
        {
            Position = _Position;
            Normal = _Normal;
            TexCoord = _TexCoord;
        }
    }
}
