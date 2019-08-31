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

		public void Begin()
		{
			var device = Nrs.GraphicsDevice;
			_depthStencilState = device.DepthStencilState;
			_rasterizerState = device.RasterizerState;
			_blendState = device.BlendState;

			device.BlendState = BlendState.AlphaBlend;
			device.DepthStencilState = DepthStencilState.Default;
			device.RasterizerState = RasterizerState.CullNone;

			_beginCalled = true;
		}

		public void DrawModel(Sprite3D model, RenderContext context)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			context.ViewProjection = context.View * context.Projection;
			model.UpdateNodesAbsoluteTransforms();
			using (var transformScope = new TransformScope(context, model.Transform))
			{
				foreach (var mesh in model.Meshes)
				{
					mesh.Draw(context);
				}
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
