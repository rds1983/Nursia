using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D.ForwardRendering
{
	internal class WaterRenderer
	{
		private readonly MeshData _waterMesh;
		private DateTime? _lastRenderTime;

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

				var effect = Resources.GetWaterEffect(Nrs.DepthBufferEnabled, waterTile.CubeMapReflection);

				// Textures
				effect.Parameters["_textureNormals1"].SetValue(Resources.WaterNormals1);
				effect.Parameters["_textureNormals2"].SetValue(Resources.WaterNormals2);
				effect.Parameters["_textureScreen"].SetValue(context.Screen);
				effect.Parameters["_textureReflection"].SetValue(waterTile.TargetReflection);
				effect.Parameters["_textureDepth"].SetValue(context.Depth);
				effect.Parameters["_textureSkybox"].SetValue(context.Scene.Skybox.Texture);

				// Offsets
				effect.Parameters["_time"].SetValue(deltaTime);

				// Lights
				context.SetLights(effect);
				effect.Parameters["_specularPower"].SetValue(waterTile.SpecularPower);
				effect.Parameters["_specularFactor"].SetValue(waterTile.SpecularFactor);

				// Water parameters
				effect.Parameters["_color1"].SetValue(waterTile.Color1.ToVector4());
				effect.Parameters["_color2"].SetValue(waterTile.Color2.ToVector4());
				effect.Parameters["_colorDeep"].SetValue(waterTile.ColorDeep.ToVector4());
				effect.Parameters["_colorShallow"].SetValue(waterTile.ColorShallow.ToVector4());
				effect.Parameters["_waveDirection1"].SetValue(waterTile.WaveDirection1);
				effect.Parameters["_waveDirection2"].SetValue(waterTile.WaveDirection2);
				effect.Parameters["_timeScale"].SetValue(waterTile.TimeScale);
				effect.Parameters["_reflectionFactor"].SetValue(waterTile.ReflectionFactor);
				effect.Parameters["_fresnelFactor"].SetValue(waterTile.FresnelFactor);
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

				effect.CurrentTechnique = effect.Techniques[waterTile.RenderMode.ToString()];
				device.DrawIndexedPrimitives(effect, _waterMesh);
				++context.Statistics.MeshesDrawn;
			}
		}
	}
}
