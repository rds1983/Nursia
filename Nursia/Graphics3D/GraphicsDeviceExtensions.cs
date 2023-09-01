using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public static class GraphicsDeviceExtensions
	{
		public static void Apply(this GraphicsDevice device, MeshData meshData)
		{
			device.SetVertexBuffer(meshData.VertexBuffer);
			device.Indices = meshData.IndexBuffer;
		}

		public static void DrawIndexedPrimitives(this GraphicsDevice device, Effect effect, MeshData meshData, int startIndex = 0)
		{
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
					0,
					meshData.VertexCount,
					0,
					meshData.PrimitiveCount);
			}
		}
	}
}
