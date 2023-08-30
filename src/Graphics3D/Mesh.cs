using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Nursia.Graphics3D
{
	public class Mesh : IDisposable
	{
		private VertexBuffer _vertexBuffer;
		private bool _hasNormals;

		public int VertexCount => VertexBuffer.VertexCount;
		public VertexBuffer VertexBuffer
		{
			get => _vertexBuffer;

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_vertexBuffer = value;
				_hasNormals = (from el in _vertexBuffer.VertexDeclaration.GetVertexElements() where el.VertexElementUsage == VertexElementUsage.Normal select el).Count() > 0;
			}
		}
		
		public IndexBuffer IndexBuffer { get; set; }
		public int PrimitiveCount => IndexBuffer.IndexCount / 3;

		public bool HasNormals => _hasNormals;


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

			return new Mesh
			{
				VertexBuffer = vertexBuffer,
				IndexBuffer = indexBuffer
			};
		}
	}
}
