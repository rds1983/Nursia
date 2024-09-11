using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;

namespace Nursia.Utilities
{
	public static class GraphicsDeviceExtensions
	{
		public static void DrawIndexedPrimitives(this GraphicsDevice device, Effect effect, Mesh mesh)
		{
			device.SetVertexBuffer(mesh.VertexBuffer);
			device.Indices = mesh.IndexBuffer;

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawIndexedPrimitives(mesh.PrimitiveType, 0,
					0,
					mesh.VertexCount,
					0,
					mesh.PrimitiveCount);
			}
		}
	}
}
