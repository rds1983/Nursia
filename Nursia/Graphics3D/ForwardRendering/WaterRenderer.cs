using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D.ForwardRendering
{
	internal class WaterRenderer
	{
		private readonly MeshData _waterMesh;
		private DateTime? _lastRenderTime;
		private float _moveFactor = 0;

		public WaterRenderer()
		{
			_waterMesh = PrimitiveMeshes.SquarePositionTextureFromZeroToOne;
		}

		public void DrawWater(RenderContext context)
		{
			var device = Nrs.GraphicsDevice;

			// Update move factor
			var now = DateTime.Now;
			var deltaTime = 0.0f;
			if (_lastRenderTime != null)
			{
				deltaTime = (float)(now - _lastRenderTime.Value).TotalSeconds;
			} else
			{
				_lastRenderTime = now;
			}

			var scene = context.Scene;
			foreach (var waterTile in scene.WaterTiles)
			{
				if (context.Frustrum.Contains(waterTile.BoundingBox) == ContainmentType.Disjoint)
				{
					continue;
				}

				var effect = Resources.GetWaterEffect(Nrs.DepthBufferEnabled, waterTile.CubeMapReflection)();

				// Textures
				effect.Parameters["_textureDudv"].SetValue(Resources.WaterDudv);
				effect.Parameters["_textureNormals"].SetValue(Resources.WaterNormals);
				effect.Parameters["_textureRefraction"].SetValue(context.Screen);
				effect.Parameters["_textureReflection"].SetValue(waterTile.TargetReflection);
				effect.Parameters["_textureDepth"].SetValue(context.Depth);
				effect.Parameters["_textureSkybox"].SetValue(context.Scene.Skybox.Texture);

				// Offsets
				_moveFactor += waterTile.WaveStrength * (float)Nrs.Game.TargetElapsedTime.TotalSeconds;
				_moveFactor %= 1.0f;
				effect.Parameters["_moveFactor"].SetValue(_moveFactor);
				effect.Parameters["_tiling"].SetValue(4.0f);

				// Lights
				context.SetLights(effect);
				effect.Parameters["_specularPower"].SetValue(waterTile.SpecularPower);
				effect.Parameters["_specularFactor"].SetValue(waterTile.SpecularFactor);

				// Water parameters
				effect.Parameters["_colorDeep"].SetValue(waterTile.ColorDeep.ToVector4());
				effect.Parameters["_colorShallow"].SetValue(waterTile.ColorShallow.ToVector4());
				effect.Parameters["_tiling"].SetValue(waterTile.Tiling);
				effect.Parameters["_waveStrength"].SetValue(waterTile.WaveStrength);
				effect.Parameters["_edgeFactor"].SetValue(waterTile.EdgeFactor);
				effect.Parameters["_murkinessStart"].SetValue(waterTile.MurkinessStart);
				effect.Parameters["_murkinessFactor"].SetValue(waterTile.MurkinessFactor);

				// Render parameters
				var world = Matrix.CreateScale(waterTile.SizeX, 1, waterTile.SizeZ) *
					Matrix.CreateTranslation(waterTile.X, waterTile.Height, waterTile.Z);
				effect.Parameters["_world"].SetValue(world);
				var worldViewProj = world * context.ViewProjection;
				effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
				effect.Parameters["_far"].SetValue(context.FarPlaneDistance);
				effect.Parameters["_near"].SetValue(context.NearPlaneDistance);
				effect.Parameters["_cameraPosition"].SetValue(context.Scene.Camera.Position);

				// Compute reflection view matrix
				Vector3 reflCameraPosition = context.Scene.Camera.Position;
				reflCameraPosition.Y = -context.Scene.Camera.Position.Y + waterTile.Height * 2;
				Vector3 reflTargetPos = context.Scene.Camera.Target;
				reflTargetPos.Y = -context.Scene.Camera.Target.Y + waterTile.Height * 2;
				Vector3 invUpVector = Vector3.Cross(context.Scene.Camera.Right, reflTargetPos - reflCameraPosition);
				var reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
				var reflectProjectWorld = world * reflectionViewMatrix * context.Projection;
				effect.Parameters["_reflectViewProj"].SetValue(reflectProjectWorld);

				device.Apply(_waterMesh);

				//effect.CurrentTechnique = effect.Techniques[waterTile.RenderMode.ToString()];
				device.DrawIndexedPrimitives(effect, _waterMesh);
				++context.Statistics.MeshesDrawn;
			}
		}
	}
}
