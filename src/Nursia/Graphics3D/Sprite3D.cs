using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Materials;

namespace Nursia.Graphics3D.Modeling
{
	public partial class Sprite3D
	{
		public PrimitiveType PrimitiveType { get; private set; }

		public int PrimitiveCount { get; private set; }

		public BaseMaterial Material { get; set; }

		public VertexDeclaration VertexDeclaration { get; private set; }

		public VertexBuffer VertexBuffer { get; private set; }

		public IndexBuffer IndexBuffer { get; private set; }

		public void Init<T>(GraphicsDevice device, T[] vertices, int[] indices, PrimitiveType primitiveType) where T: struct, IVertexType
		{
			VertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
													BufferUsage.WriteOnly);
			VertexBuffer.SetData(vertices);

			IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
			IndexBuffer.SetData(indices);

			PrimitiveType = primitiveType;
			VertexDeclaration = new T().VertexDeclaration;
			PrimitiveCount = CalculatePrimitiveCount(PrimitiveType, indices.Length);
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
