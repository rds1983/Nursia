using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Scene;
using System;

namespace Nursia.Graphics3D
{
	public class ForwardRenderer
	{
		private DepthStencilState _depthStencilState;
		private RasterizerState _rasterizerState;
		private BlendState _blendState;
		private bool _beginCalled;
		private readonly RenderContext _renderContext = new RenderContext();

		public RenderContext RenderContext
		{
			get
			{
				return _renderContext;
			}
		}

		public void Begin()
		{
			var device = Nrs.GraphicsDevice;
			_depthStencilState = device.DepthStencilState;
			_rasterizerState = device.RasterizerState;
			_blendState = device.BlendState;

			device.DepthStencilState = DepthStencilState.Default;
			device.RasterizerState = RasterizerState.CullNone;

			_beginCalled = true;
		}

		public void DrawModel(Sprite3D model, Camera camera)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			_renderContext.Transform = model.Transform;
			_renderContext.Camera = camera;

			model.UpdateBoneNodesAbsoluteTransforms();
			foreach (var mesh in model.Meshes)
			{
				mesh.Draw(_renderContext);
			}
		}

		public void End()
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			var device = Nrs.GraphicsDevice;
			device.DepthStencilState = _depthStencilState;
			_depthStencilState = null;
			device.RasterizerState = _rasterizerState;
			_rasterizerState = null;
			device.BlendState = _blendState;
			_blendState = null;

			_beginCalled = false;
		}
	}
}
