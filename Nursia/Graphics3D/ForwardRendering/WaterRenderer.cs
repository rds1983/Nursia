﻿using Microsoft.Xna.Framework;
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
			}

			_lastRenderTime = now;

			var scene = context.Scene;
			foreach (var waterTile in scene.WaterTiles)
			{
				if (context.Frustrum.Contains(waterTile.BoundingBox) == ContainmentType.Disjoint)
				{
					continue;
				}

				var effect = Resources.GetWaterEffect(waterTile.Waves, Nrs.DepthBufferEnabled);

				// Textures
				effect.Parameters["_textureWave0"].SetValue(Resources.WaterWave0);
				effect.Parameters["_textureWave1"].SetValue(Resources.WaterWave1);

				effect.Parameters["_textureRefraction"].SetValue(waterTile.TargetRefraction);
				effect.Parameters["_textureReflection"].SetValue(waterTile.TargetReflection);
				effect.Parameters["_textureDepth"].SetValue(waterTile.TargetDepth);

				// Offsets
				waterTile.WaveMapOffset0 += waterTile.WaveVelocity0 * deltaTime;
				waterTile.WaveMapOffset1 += waterTile.WaveVelocity1 * deltaTime;

				if (waterTile.WaveMapOffset0.X >= 1.0f || waterTile.WaveMapOffset0.X <= -1.0f)
					waterTile.WaveMapOffset0.X = 0.0f;
				if (waterTile.WaveMapOffset1.X >= 1.0f || waterTile.WaveMapOffset1.X <= -1.0f)
					waterTile.WaveMapOffset1.X = 0.0f;
				if (waterTile.WaveMapOffset0.Y >= 1.0f || waterTile.WaveMapOffset0.Y <= -1.0f)
					waterTile.WaveMapOffset0.Y = 0.0f;
				if (waterTile.WaveMapOffset1.Y >= 1.0f || waterTile.WaveMapOffset1.Y <= -1.0f)
					waterTile.WaveMapOffset1.Y = 0.0f;
				effect.Parameters["_waveMapOffset0"].SetValue(waterTile.WaveMapOffset0);
				effect.Parameters["_waveMapOffset1"].SetValue(waterTile.WaveMapOffset1);


				// Lights
				context.SetLights(effect);
				effect.Parameters["_specularPower"].SetValue(waterTile.SpecularPower);
				effect.Parameters["_specularFactor"].SetValue(waterTile.SpecularFactor);

				// Water parameters
				effect.Parameters["_waterColor"].SetValue(waterTile.Color.ToVector4());
				effect.Parameters["_reflectionFactor"].SetValue(waterTile.ReflectionFactor);
				effect.Parameters["_waveTextureScale"].SetValue(waterTile.WaveTextureScale);
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
