using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public class Mesh
	{
		public PrimitiveType PrimitiveType { get; set; }
		public VertexBuffer VertexBuffer { get; set; }
		public IndexBuffer IndexBuffer { get; set; }

		public int PrimitiveCount
		{
			get
			{
				return CalculatePrimitiveCount(PrimitiveType, IndexBuffer.IndexCount);
			}
		}

		public static Mesh Create<T>(T[] vertices, short[] indices, PrimitiveType primitiveType) where T : struct, IVertexType
		{
			var vertexBuffer = new VertexBuffer(Nrs.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
													BufferUsage.None);
			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);

			return new Mesh
			{
				VertexBuffer = vertexBuffer,
				IndexBuffer = indexBuffer,
				PrimitiveType = primitiveType
			};
		}

		public static int CalculatePrimitiveCount(PrimitiveType primitiveType, int indices)
		{
			var result = 0;

			switch (primitiveType)
			{
				case PrimitiveType.TriangleList:
					result = indices / 3;
					break;
				case PrimitiveType.LineList:
					result = indices / 2;
					break;
			}

			return result;
		}
	}
}