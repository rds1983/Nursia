using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;
using System;

namespace Nursia.Graphics3D
{
	public class ForwardRenderer
	{
		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private SamplerState _oldSamplerState;
		private bool _beginCalled;
		private readonly RenderContext _context = new RenderContext();

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullClockwise;
		public BlendState BlendState { get; set; } = BlendState.AlphaBlend;
		public SamplerState SamplerState { get; set; } = SamplerState.LinearWrap;

		public RenderStatistics Statistics
		{
			get
			{
				return _context.Statistics;
			}
		}

		public void Begin()
		{
			var device = Nrs.GraphicsDevice;
			_oldDepthStencilState = device.DepthStencilState;
			_oldRasterizerState = device.RasterizerState;
			_oldBlendState = device.BlendState;
			_oldSamplerState = device.SamplerStates[0];

			device.BlendState = BlendState;
			device.DepthStencilState = DepthStencilState;
			device.RasterizerState = RasterizerState;
			device.SamplerStates[0] = SamplerState;

			_beginCalled = true;

			_context.Statistics.Reset();
		}

		private void DrawModel(Sprite3D model)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			model.UpdateNodesAbsoluteTransforms();
			using (var transformScope = new TransformScope(_context, model.Transform))
			{
				foreach (var mesh in model.Meshes)
				{
					mesh.Draw(_context);
				}
			}
		}

		public void DrawScene(Scene scene)
		{
			if (Nrs.GraphicsDevice.Viewport.Height == 0)
			{
				return;
			}

			_context.Scene = scene;
			_context.View = scene.Camera.View;
			_context.Projection = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.ToRadians(scene.Camera.ViewAngle),
					Nrs.GraphicsDevice.Viewport.AspectRatio,
					0.1f,
					1000.0f);

			foreach(var model in scene.Models)
			{
				DrawModel(model);
			}
		}

		public void End()
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			var device = Nrs.GraphicsDevice;
			device.DepthStencilState = _oldDepthStencilState;
			_oldDepthStencilState = null;
			device.RasterizerState = _oldRasterizerState;
			_oldRasterizerState = null;
			device.BlendState = _oldBlendState;
			_oldBlendState = null;
			device.SamplerStates[0] = _oldSamplerState;
			_oldSamplerState = null;

			_beginCalled = false;
		}
	}
}
