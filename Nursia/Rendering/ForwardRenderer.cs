using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public class ForwardRenderer
	{
		private int[] _effectLightType = new int[Constants.MaxLights];
		private Vector3[] _effectLightPosition = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightDirection = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightColor = new Vector3[Constants.MaxLights];
		private int _lightCount = 0;

		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private RenderTargetUsage _oldRenderTargetUsage;
		private readonly RenderContext _context = new RenderContext();

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;
		public BlendState BlendState { get; set; } = BlendState.NonPremultiplied;

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

			// Process render jobs
			var device = Nrs.GraphicsDevice;

			List<RenderJob> colorJob;
			if (_context.Jobs.TryGetValue("Default", out colorJob))
			{
				foreach (var job in colorJob)
				{
					var commonPars = job.Material.CommonParameters;

					commonPars.World?.SetValue(job.Transform);

					if (commonPars.WorldViewProj != null)
					{
						var worldViewProj = job.Transform * _context.ViewProjection;
						commonPars.WorldViewProj.SetValue(worldViewProj);
					}

					if (commonPars.WorldInverseTranspose != null)
					{
						var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(job.Transform));
						commonPars.WorldInverseTranspose.SetValue(worldInverseTranspose);
					}

					commonPars.CameraPosition?.SetValue(camera.Position);
					commonPars.LightType?.SetValue(_effectLightType);
					commonPars.LightPosition?.SetValue(_effectLightPosition);
					commonPars.LightDirection?.SetValue(_effectLightDirection);
					commonPars.LightColor?.SetValue(_effectLightColor);
					commonPars.LightCount?.SetValue(_lightCount);

					job.Material.SetMaterialParameters();

					device.Apply(job.Mesh);
					Nrs.GraphicsDevice.DrawIndexedPrimitives(job.Material.Effect.Techniques[job.TechniqueName], job.Mesh);

					++_context.Statistics.MeshesDrawn;
				}
			}

			// Restore state
			RestoreState();
		}
	}
}
