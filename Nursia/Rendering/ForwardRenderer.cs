using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using Nursia.Rendering.Lights;
using System;
using glTFLoader.Schema;

namespace Nursia.Rendering
{
	public class ForwardRenderer
	{
		private const int ShadowMapSize = 2048;

		private enum RenderPassType
		{
			ShadowMap,
			Opaque,
			Transparent
		}

		private int[] _effectLightType = new int[Constants.MaxLights];
		private Vector3[] _effectLightPosition = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightDirection = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightColor = new Vector3[Constants.MaxLights];
		private int _lightCount = 0;
		private Matrix _lightViewProj;

		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private RenderTargetUsage _oldRenderTargetUsage;
		private readonly RenderBatch _batch = new RenderBatch();

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;
		public BlendState BlendState { get; set; } = BlendState.Opaque;

		public BaseLight ShadowCastingLight { get; private set; }

		public RenderTarget2D ShadowMap { get; private set; }


		public ForwardRenderer()
		{
		}

		private void SetState()
		{
			if (ShadowMap == null)
			{
				ShadowMap = new RenderTarget2D(Nrs.GraphicsDevice,
									ShadowMapSize, ShadowMapSize,
									false, SurfaceFormat.Single,
									DepthFormat.Depth24);
			}

			var device = Nrs.GraphicsDevice;
			_oldDepthStencilState = device.DepthStencilState;
			_oldRasterizerState = device.RasterizerState;
			_oldBlendState = device.BlendState;
			_oldRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;

			device.BlendState = BlendState;
			device.DepthStencilState = DepthStencilState;
			device.RasterizerState = RasterizerState;

			device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		}

		private void RestoreState()
		{
			var device = Nrs.GraphicsDevice;
			device.DepthStencilState = _oldDepthStencilState;
			_oldDepthStencilState = null;
			device.RasterizerState = _oldRasterizerState;
			_oldRasterizerState = null;
			device.BlendState = _oldBlendState;
			_oldBlendState = null;
			device.PresentationParameters.RenderTargetUsage = _oldRenderTargetUsage;
		}

		private void BatchNode(RenderBatch batch, SceneNode node)
		{
			node.Render(batch);

			foreach (var child in node.Children)
			{
				BatchNode(batch, child);
			}
		}

		private void SetLights()
		{
			_lightCount = 0;
			foreach (var directLight in _batch.DirectLights)
			{
				if (_lightCount >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[_lightCount] = 0;
				_effectLightColor[_lightCount] = directLight.Color.ToVector3();
				_effectLightDirection[_lightCount] = directLight.NormalizedDirection;
				++_lightCount;
			}

			foreach (var pointLight in _batch.PointLights)
			{
				if (_lightCount >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[_lightCount] = 1;
				_effectLightColor[_lightCount] = pointLight.Color.ToVector3();
				_effectLightPosition[_lightCount] = pointLight.Translation;

				++_lightCount;
			}
		}

		private void RenderPass(Camera camera, RenderPassType passType)
		{
			var device = Nrs.GraphicsDevice;

			EffectBinding lastBinding = null;
			foreach (var job in _batch.Jobs)
			{
				switch (passType)
				{
					case RenderPassType.Opaque:
						if (job.Material.BlendMode != NodeBlendMode.Opaque)
						{
							continue;
						}

						break;
					case RenderPassType.Transparent:
						if (job.Material.BlendMode != NodeBlendMode.Transparent)
						{
							continue;
						}

						break;
				}

				var effectBinding = job.Material.EffectBinding;
				if (lastBinding == null || effectBinding.BatchId != lastBinding.BatchId)
				{
					// Effect level params
					effectBinding.LightViewProj?.SetValue(_lightViewProj);
					effectBinding.View?.SetValue(camera.View);
					effectBinding.CameraPosition?.SetValue(camera.Position);
					effectBinding.LightType?.SetValue(_effectLightType);
					effectBinding.LightPosition?.SetValue(_effectLightPosition);
					effectBinding.LightDirection?.SetValue(_effectLightDirection);
					effectBinding.LightColor?.SetValue(_effectLightColor);
					effectBinding.LightCount?.SetValue(_lightCount);

					if (passType != RenderPassType.ShadowMap)
					{
						effectBinding.ShadowMap?.SetValue(ShadowMap);
					}

					lastBinding = effectBinding;
				}

				effectBinding.World?.SetValue(job.Transform);

				if (effectBinding.WorldViewProj != null)
				{
					var worldViewProj = job.Transform * _batch.ViewProjection;
					effectBinding.WorldViewProj.SetValue(worldViewProj);
				}

				if (effectBinding.WorldInverseTranspose != null)
				{
					var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(job.Transform));
					effectBinding.WorldInverseTranspose.SetValue(worldInverseTranspose);
				}

				if (job.Mesh.HasBones)
				{
					effectBinding.Bones?.SetValue(job.Mesh.BonesTransforms);
				}

				job.Material.SetParameters();

				device.DrawIndexedPrimitives(effectBinding.Effect, job.Mesh);
			}
		}

		private void ShadowMapRun(SceneNode node, Camera camera)
		{
			if (ShadowCastingLight == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var oldViewport = device.Viewport;
			try
			{
				// Light camera
				var lightCamera = ShadowCastingLight.GetLightCamera(camera.Position);

				// Batch render jobs
				_batch.PrepareRender(RenderBatchPass.ShadowMap, lightCamera);
				BatchNode(_batch, node);

				_lightViewProj = _batch.ViewProjection;

				// Render the shadow map
				device.SetRenderTarget(ShadowMap);

				// Clear the render target to white or all 1's
				// We set the clear to white since that represents the 
				// furthest the object could be away
				device.Clear(Color.White);

				// Shadow map pass
				RenderPass(lightCamera, RenderPassType.ShadowMap);
			}
			finally
			{
				// Set render target back to the back buffer
				device.SetRenderTarget(null);
				device.Viewport = oldViewport;
			}
		}

		public void Render(SceneNode node, Camera camera)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (camera == null)
			{
				throw new ArgumentNullException(nameof(camera));
			}

			var device = Nrs.GraphicsDevice;
			if (device.Viewport.Width == 0 || device.Viewport.Height == 0)
			{
				// Can't render
				return;
			}

			// Set state
			SetState();

			// Firstly determine whether we have a shadow casting light
			ShadowCastingLight = (BaseLight)node.QueryFirst(n =>
			{
				var asLight = n as BaseLight;
				return asLight != null && asLight.RenderCastsShadow;
			});

			// Shadow map run
			ShadowMapRun(node, camera);

			// Main run
			_batch.PrepareRender(RenderBatchPass.Main, camera);

			// Batch Render Jobs
			BatchNode(_batch, node);

			// Set light parameters
			SetLights();

			// Opaque run
			RenderPass(camera, RenderPassType.Opaque);

			// Transparent Run
			device.BlendState = BlendState.NonPremultiplied;
			device.RasterizerState = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.DepthRead;
			RenderPass(camera, RenderPassType.Transparent);

			// Restore state
			RestoreState();

			if (DebugSettings.DrawBoundingBoxes)
			{
				foreach (var job in _batch.Jobs)
				{
					var t = job.Transform;
					var bb = job.Mesh.BoundingBox.Transform(ref t);
					DebugShapeRenderer.AddBoundingBox(bb, Color.Blue);
				}
			}

			if (DebugSettings.DrawLightViewFrustrum && ShadowCastingLight != null)
			{
				var frustum = new BoundingFrustum(_lightViewProj);
				DebugShapeRenderer.AddBoundingFrustum(frustum, Color.Green);
			}

			DebugShapeRenderer.Draw(camera.View, _batch.Projection);
		}
	}
}
