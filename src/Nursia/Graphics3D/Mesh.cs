using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public class Mesh
	{
		public VertexBuffer VertexBuffer { get; set; }
		public IndexBuffer IndexBuffer { get; set; }

		public int PrimitiveCount
		{
			get
			{
				return IndexBuffer.IndexCount / 3;
			}
		}

		internal static Mesh Create<T>(T[] vertices, short[] indices, 
			PrimitiveType primitiveType) where T : struct, IVertexType
		{
			var vertexBuffer = new VertexBuffer(Nrs.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
													BufferUsage.None);
			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);

			return new Mesh
			{
				VertexBuffer = vertexBuffer,
				IndexBuffer = indexBuffer
			};
		}
	}
}
