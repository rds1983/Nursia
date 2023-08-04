using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Graphics3D
{
	public class Mesh : IDisposable
	{
		public int VertexCount => VertexBuffers[0].VertexBuffer.VertexCount;
		public VertexBufferBinding[] VertexBuffers { get; set; }
		public IndexBuffer IndexBuffer { get; set; }

		public int PrimitiveCount
		{
			get
			{
				return IndexBuffer.IndexCount / 3;
			}
		}

		public Mesh()
		{
		}

		~Mesh()
		{
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
		}

		public static Mesh Create<T>(T[] vertices,
			short[] indices,
			PrimitiveType primitiveType = PrimitiveType.TriangleList) where T : struct, IVertexType
		{
			var device = Nrs.GraphicsDevice;
			var vertexBuffer = new VertexBuffer(device,
				new T().VertexDeclaration,
				vertices.Length,
				BufferUsage.None);

			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(device,
				IndexElementSize.SixteenBits,
				indices.Length,
				BufferUsage.None);
			indexBuffer.SetData(indices);

			var vbb = new VertexBufferBinding(vertexBuffer);
			return new Mesh
			{
				VertexBuffers = new VertexBufferBinding[] { vbb },
				IndexBuffer = indexBuffer
			};
		}
	}
}
