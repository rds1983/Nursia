using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public static class GraphicsDeviceExtensions
	{
		public static void Apply(this GraphicsDevice device, Mesh mesh)
		{
			device.SetVertexBuffer(mesh.VertexBuffer);
			device.Indices = mesh.IndexBuffer;
		}

		public static void DrawIndexedPrimitives(
			this GraphicsDevice device, 
			Effect effect,
			Mesh mesh,
			int? vertexCount = null,
			int startIndex = 0,
			int? primitiveCount = null)
		{
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 
					0,
					vertexCount ?? mesh.VertexBuffer.VertexCount, 
					startIndex, 
					primitiveCount ?? mesh.PrimitiveCount);
			}
		}
	}
}
