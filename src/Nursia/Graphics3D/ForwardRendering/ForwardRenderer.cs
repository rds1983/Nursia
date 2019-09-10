using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.CommonRendering;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Water;
using System;

namespace Nursia.Graphics3D.ForwardRendering
{
	public partial class ForwardRenderer
	{
		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private SamplerState _oldSamplerState;
		private bool _beginCalled;
		private readonly RenderContext _context = new RenderContext();
		private WaterRenderer _waterRenderer;

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

		public ForwardRenderer()
		{
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

		internal void DrawMeshNode(MeshNode meshNode)
		{
			foreach (var part in meshNode.Parts)
			{
				var boundingSphere = part.BoundingSphere.Transform(meshNode.AbsoluteTransform);
				if (_context.Frustrum.Contains(boundingSphere) == ContainmentType.Disjoint)
				{
					continue;
				}

				// If part has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				using (var scope = new TransformScope(_context,
					part.Bones.Count > 0 ? Matrix.Identity : meshNode.AbsoluteTransform))
				{
					DrawMeshPart(part);
				}
			}
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
					DrawMeshNode(mesh);
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

			foreach (var model in scene.Models)
			{
				DrawModel(model);
			}

			if (scene.WaterTiles.Count > 0)
			{
				if (_waterRenderer == null)
				{
					_waterRenderer = new WaterRenderer();
				}

				_waterRenderer.DrawWater(_context);
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