using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using Nursia.Rendering.Lights;

namespace Nursia.Rendering
{
	public class ForwardRenderer
	{
		private enum RenderPassType
		{
			ShadowMap,
			Default
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
		private RenderTarget2D _shadowMap;
		private readonly RenderContext _context = new RenderContext();

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;
		public BlendState BlendState { get; set; } = BlendState.Opaque;

		public RenderStatistics Statistics => _context.Statistics;

		public ForwardRenderer()
		{
		}

		private void SetState()
		{
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
				EffectBinding effectBinding = null;
				switch (passType)
				{
					case RenderPassType.ShadowMap:
						effectBinding = job.Material.ShadowMapEffect;
						break;
					case RenderPassType.Default:
						effectBinding = job.Material.DefaultEffect;
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
					if (effectBinding.ShadowMap != null)
					{
						effectBinding.ShadowMap.SetValue(_shadowMap);
					}
				}

				effectBinding.SetMaterialParams(job.Material);

				device.Apply(job.Mesh);
				Nrs.GraphicsDevice.DrawIndexedPrimitives(effectBinding.Effect, job.Mesh);

				++_context.Statistics.MeshesDrawn;
			}
		}

		private void ShadowMapRun(BaseLight light, Camera camera)
		{
			if (light.ShadowMap == null || !light.ShadowMapDirty)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;
			var oldViewport = device.Viewport;
			try
			{
				device.SetRenderTarget(light.ShadowMap);

				// Clear the render target to white or all 1's
				// We set the clear to white since that represents the 
				// furthest the object could be away
				device.Clear(Color.White);

				// Shadow map pass
				_lightViewProj = light.CreateLightViewProjectionMatrix(_context.Camera);
				RenderPass(RenderPassType.ShadowMap);
			}
			finally
			{
				// Set render target back to the back buffer
				device.SetRenderTarget(null);
				device.Viewport = oldViewport;
			}

			_shadowMap = light.ShadowMap;
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

			_shadowMap = null;

			// Shadow map runs
			foreach(var directLight in _context.DirectLights)
			{
				ShadowMapRun(directLight, camera);
			}

			// Default run
			RenderPass(RenderPassType.Default);

			// Restore state
			RestoreState();
		}
	}
}
