using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Graphics3D.ForwardRendering
{
	internal class WaterRenderer
	{
		private readonly Point TargetRefractionSize = new Point(1280, 720);
		private readonly Point TargetReflectionSize = new Point(640, 360);
		private const float WaveSpeed = 0.03f;

		private readonly Mesh _waterMesh;
		private readonly RenderTarget2D _targetRefraction;
		private readonly RenderTarget2D _targetReflection;
		private DateTime? _lastRenderTime;
		private float _moveFactor = 0;

		public RenderTarget2D TargetRefraction
		{
			get
			{
				return _targetRefraction;
			}
		}

		public RenderTarget2D TargetReflection
		{
			get
			{
				return _targetReflection;
			}
		}

		public WaterRenderer()
		{
			// Create water tile
			// Water effect ignores y
			// So it's always set to zero
			var vertices = new VertexPositionTexture[]
			{
				new VertexPositionTexture(new Vector3(-1, 0, -1), Vector2.Zero),
				new VertexPositionTexture(new Vector3(-1, 0, 1), Vector2.Zero),
				new VertexPositionTexture(new Vector3(1, 0, -1), Vector2.Zero),
				new VertexPositionTexture(new Vector3(1, 0, -1), Vector2.Zero),
				new VertexPositionTexture(new Vector3(-1, 0, 1), Vector2.Zero),
				new VertexPositionTexture(new Vector3(1, 0, 1), Vector2.Zero)
			};

			var indices = new short[]
			{
				0, 1, 2, 3, 4, 5
			};

			_waterMesh = Mesh.Create(vertices, indices);

			_targetRefraction = new RenderTarget2D(Nrs.GraphicsDevice,
				TargetRefractionSize.X,
				TargetRefractionSize.Y,
				false,
				SurfaceFormat.Color,
				DepthFormat.Depth24);

			_targetReflection = new RenderTarget2D(Nrs.GraphicsDevice,
				TargetReflectionSize.X,
				TargetReflectionSize.Y,
				false,
				SurfaceFormat.Color,
				DepthFormat.Depth24);
		}

		public void DrawWater(RenderContext context)
		{
			var device = Nrs.GraphicsDevice;
			var effect = Assets.GetWaterEffect();

			// Update move factor
			var now = DateTime.Now;
			if (_lastRenderTime != null)
			{
				var passed = (float)(now - _lastRenderTime.Value).TotalSeconds;
				_moveFactor += WaveSpeed * passed;
				_moveFactor %= 1.0f;
			}

			_lastRenderTime = now;

			effect.Parameters["_cameraPosition"].SetValue(context.Scene.Camera.Position);
			effect.Parameters["_moveFactor"].SetValue(_moveFactor);
			effect.Parameters["_lightPosition"].SetValue(new Vector3(10000, 10000, -10000));
			effect.Parameters["_lightColor"].SetValue(Color.White.ToVector3());
			effect.Parameters["_textureDUDV"].SetValue(Assets.WaterDUDV);
			effect.Parameters["_textureNormals"].SetValue(Assets.WaterNormals);
			effect.Parameters["_textureRefraction"].SetValue(TargetRefraction);
			effect.Parameters["_textureReflection"].SetValue(TargetReflection);
			var scene = context.Scene;
			foreach (var waterTile in scene.WaterTiles)
			{
				var world = Matrix.CreateScale(waterTile.Size) *
					Matrix.CreateTranslation(waterTile.X, 
						waterTile.Height, 
						waterTile.Z);

				effect.Parameters["_world"].SetValue(world);
				effect.Parameters["_viewProjection"].SetValue(context.ViewProjection);

				device.Apply(_waterMesh);
				device.DrawIndexedPrimitives(effect, _waterMesh);
				++context.Statistics.MeshesDrawn;
			}
		}
	}
}
