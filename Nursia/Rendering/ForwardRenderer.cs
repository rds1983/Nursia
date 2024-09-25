using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using Nursia.Rendering.Lights;

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
		private Matrix? _lightViewProj;

		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private RenderTargetUsage _oldRenderTargetUsage;
		private readonly RenderContext _context = new RenderContext();

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;
		public BlendState BlendState { get; set; } = BlendState.Opaque;

		public RenderStatistics Statistics => _context.Statistics;

		public BaseLight ShadowCastingLight { get; private set; }

		public RenderContext Context => _context;
		private RenderTarget2D ShadowMap { get; set; }

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

		private void BatchNode(SceneNode node)
		{
			node.Render(_context);

			foreach (var child in node.Children)
			{
				BatchNode(child);
			}
		}

		private void SetLights()
		{
			_lightCount = 0;
			foreach (var directLight in _context.DirectLights)
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

			foreach (var pointLight in _context.PointLights)
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

		private void RenderPass(RenderPassType passType)
		{
			var device = Nrs.GraphicsDevice;
			var camera = _context.Camera;

			foreach (var job in _context.Jobs)
			{
				if (job.Mesh == null || job.Material == null)
				{
					continue;
				}

				IMaterial material = job.Material;
				EffectBinding effectBinding = null;
				switch (passType)
				{
					case RenderPassType.ShadowMap:
						{
							if (!job.Material.CastsShadows)
							{
								continue;
							}

							effectBinding = Resources.GetShadowMapEffectBinding(job.Mesh.HasBones, false)();
							material = null;
						}
						break;
					case RenderPassType.Opaque:
						if (job.Material.BlendMode != NodeBlendMode.Opaque)
						{
							continue;
						}

						effectBinding = job.Material.EffectBinding;
						break;
					case RenderPassType.Transparent:
						if (job.Material.BlendMode != NodeBlendMode.Transparent)
						{
							continue;
						}

						effectBinding = job.Material.EffectBinding;
						break;
				}

				if (effectBinding == null)
				{
					continue;
				}

				effectBinding.World?.SetValue(job.Transform);

				if (effectBinding.WorldViewProj != null)
				{
					var worldViewProj = job.Transform * _context.ViewProjection;
					effectBinding.WorldViewProj.SetValue(worldViewProj);
				}

				if (effectBinding.WorldInverseTranspose != null)
				{
					var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(job.Transform));
					effectBinding.WorldInverseTranspose.SetValue(worldInverseTranspose);
				}

				if (_lightViewProj != null)
				{
					effectBinding.LightViewProj?.SetValue(_lightViewProj.Value);
				}

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

				if (job.Mesh.HasBones)
				{
					effectBinding.Bones?.SetValue(job.Mesh.BonesTransforms);
				}

				if (material != null)
				{
					material.SetParameters(job.Mesh);
				}

				device.DrawIndexedPrimitives(effectBinding.Effect, job.Mesh);

				++_context.Statistics.MeshesDrawn;
			}
		}

		private void ShadowMapRun()
		{
			var device = Nrs.GraphicsDevice;

			var oldViewport = device.Viewport;
			try
			{
				device.SetRenderTarget(ShadowMap);

				// Clear the render target to white or all 1's
				// We set the clear to white since that represents the 
				// furthest the object could be away
				device.Clear(Color.White);

				if (ShadowCastingLight == null)
				{
					return;
				}

				_lightViewProj = ShadowCastingLight.CreateLightViewProjectionMatrix(_context);
				if (_lightViewProj == null)
				{
					return;
				}

				// Shadow map pass
				RenderPass(RenderPassType.ShadowMap);
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
			// Prepare render context
			if (!_context.PrepareRender(camera))
			{
				return;
			}

			// Batch Render Jobs
			BatchNode(node);

			// Set state
			SetState();

			// Set light parameters
			SetLights();

			// Determine shadow casting light
			ShadowCastingLight = null;
			foreach (var light in _context.DirectLights)
			{
				if (!light.RenderCastsShadow)
				{
					continue;
				}

				ShadowCastingLight = light;
				break;
			}

			// Shadow map run
			ShadowMapRun();

			// Opaque run
			RenderPass(RenderPassType.Opaque);

			// Transparent Run
			var device = Nrs.GraphicsDevice;
			device.BlendState = BlendState.NonPremultiplied;
			device.RasterizerState = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.DepthRead;
			RenderPass(RenderPassType.Transparent);

			// Restore state
			RestoreState();

			if (DebugSettings.DrawBoundingBoxes)
			{
				foreach (var job in _context.Jobs)
				{
					var t = job.Transform;
					var bb = job.Mesh.BoundingBox.Transform(ref t);
					DebugShapeRenderer.AddBoundingBox(bb, Color.Blue);
				}
			}

			if (DebugSettings.DrawLightViewFrustrum && _lightViewProj != null)
			{
				var frustum = new BoundingFrustum(_lightViewProj.Value);

				DebugShapeRenderer.AddBoundingFrustum(frustum, Color.Green);
			}

			DebugShapeRenderer.Draw(camera.View, _context.Projection);
		}
	}
}
