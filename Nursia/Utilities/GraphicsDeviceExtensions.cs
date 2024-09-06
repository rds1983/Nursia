using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;

namespace Nursia.Utilities
{
	public static class GraphicsDeviceExtensions
	{
		public static void Apply(this GraphicsDevice device, Mesh meshData)
		{
			device.SetVertexBuffer(meshData.VertexBuffer);
			device.Indices = meshData.IndexBuffer;
		}

		public static void DrawIndexedPrimitives(this GraphicsDevice device, EffectTechnique technique, Mesh meshData, int startIndex = 0)
		{
			foreach (var pass in technique.Passes)
			{
				pass.Apply();
				device.DrawIndexedPrimitives(meshData.PrimitiveType, 0,
					0,
					meshData.VertexCount,
					0,
					meshData.PrimitiveCount);
			}
		}

		public static void DrawIndexedPrimitives(this GraphicsDevice device, Effect effect, Mesh meshData, int startIndex = 0)
		{
			device.DrawIndexedPrimitives(effect.CurrentTechnique, meshData, startIndex);
		}
	}
}
