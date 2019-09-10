using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.CommonRendering;
using Nursia.Graphics3D.Water;

namespace Nursia.Graphics3D.ForwardRendering
{
	internal class WaterRenderer
	{
		private readonly Mesh _waterMesh;

		public WaterRenderer()
		{
			// Create water tile
			// Water effect ignores y
			// So it's always set to zero
			var vertices = new VertexPosition[]
			{
				new VertexPosition(new Vector3(-1, 0, -1)),
				new VertexPosition(new Vector3(-1, 0, 1)),
				new VertexPosition(new Vector3(1, 0, -1)),
				new VertexPosition(new Vector3(1, 0, -1)),
				new VertexPosition(new Vector3(-1, 0, 1)),
				new VertexPosition(new Vector3(1, 0, 1))
			};

			var indices = new short[]
			{
				0, 1, 2, 3, 4, 5
			};

			_waterMesh = Mesh.Create(vertices, indices);
		}

		public void DrawWater(RenderContext context)
		{
			var device = Nrs.GraphicsDevice;
			var effect = Assets.GetWaterEffect();
			var scene = context.Scene;
			foreach (var waterTile in scene.WaterTiles)
			{
				var world = Matrix.CreateScale(WaterTile.Size) *
					Matrix.CreateTranslation(waterTile.X, waterTile.Height, waterTile.Z);

				var worldViewProj = world * context.ViewProjection;
				effect.Parameters["_worldViewProj"].SetValue(worldViewProj);

				device.Apply(_waterMesh);
				device.DrawIndexedPrimitives(effect, _waterMesh);
				++context.Statistics.MeshesDrawn;
			}
		}
	}
}
