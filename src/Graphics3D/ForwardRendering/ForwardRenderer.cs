using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;
using Nursia.Utilities;
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

		private WaterRenderer WaterRenderer
		{
			get
			{
				if (_waterRenderer == null)
				{
					_waterRenderer = new WaterRenderer();
				}

				return _waterRenderer;
			}
		}

		public RenderTarget2D WaterReflection
		{
			get
			{
				return WaterRenderer.TargetReflection;
			}
		}

		public RenderTarget2D WaterRefraction
		{
			get
			{
				return WaterRenderer.TargetRefraction;
			}
		}

		public float NearPlaneDistance = 0.1f;
		public float FarPlaneDistance = 1000.0f;

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

		private void DrawMeshNode(ModelNode meshNode)
		{
			if (meshNode.Parts.Count > 0)
			{
				// If part has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				var m = meshNode.Skin == null ? meshNode.AbsoluteTransform : Matrix.Identity;
				using (var scope = new TransformScope(_context, m))
				{
					Matrix[] boneTransforms = null;
					// Apply the effect and render items
					if (meshNode.Skin != null)
					{
						boneTransforms = meshNode.Skin.CalculateBoneTransforms();
					}

					m = _context.World;
					foreach (var part in meshNode.Parts)
					{
						var effect = Assets.GetDefaultEffect(_context.ClipPlane != null,
							part.Material.Texture != null,
							_context.Lights.Count > 0 && part.Mesh.HasNormals,
							meshNode.Skin != null ? 4 : 0);
						if (meshNode.Skin != null)
						{
							effect.Parameters["_bones"].SetValue(boneTransforms);
						}

						var boundingBox = part.BoundingBox.Transform(ref m);
						if (_context.Frustrum.Contains(boundingBox) == ContainmentType.Disjoint)
						{
							continue;
						}

						DrawMeshPart(effect, part);
					}
				}
			}

			foreach (var child in meshNode.Children)
			{
				DrawMeshNode(child);
			}
		}

		private void DrawModel(NursiaModel model)
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

		private void RefractionPass(Scene scene)
		{
			if (scene.Terrain != null)
			{
				var effect = Assets.GetDefaultEffect(_context.ClipPlane != null, true, _context.Lights.Count > 0, 0);

				for (var x = 0; x < scene.Terrain.TilesPerX; ++x)
				{
					for (var z = 0; z < scene.Terrain.TilesPerZ; ++z)
					{
						var terrainTile = scene.Terrain[x, z];

						var m = terrainTile.MeshPart.Transform;
						var boundingBox = terrainTile.MeshPart.BoundingBox.Transform(ref m);
						if (_context.Frustrum.Contains(boundingBox) == ContainmentType.Disjoint)
						{
							continue;
						}

						DrawMeshPart(effect, terrainTile.MeshPart);
					}
				}
			}

			foreach (var model in scene.Models)
			{
				DrawModel(model);
			}
		}

		private void ReflectionPass(Scene scene)
		{
			var skybox = scene.Skybox;
			if (skybox != null && skybox.Texture != null)
			{
				var device = Nrs.GraphicsDevice;

				device.DepthStencilState = DepthStencilState.DepthRead;
				var effect = Assets.SkyboxEffect;

				var view = _context.View;
				view.Translation = Vector3.Zero;
				var transform = view * _context.Projection;

				effect.Parameters["_transform"].SetValue(transform);
				effect.Parameters["_texture"].SetValue(skybox.Texture);

				device.Apply(skybox.Mesh);

				device.SamplerStates[0] = SamplerState.LinearClamp;
				device.DrawIndexedPrimitives(effect, skybox.Mesh);
				device.SamplerStates[0] = SamplerState;

				++_context.Statistics.MeshesDrawn;

				device.DepthStencilState = DepthStencilState;
			}

			RefractionPass(scene);
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
				NearPlaneDistance, FarPlaneDistance);

			if (scene.WaterTiles.Count > 0)
			{
				// Render reflection texture
				var waterRenderer = WaterRenderer;
				var device = Nrs.GraphicsDevice;
				var oldViewport = device.Viewport;
				try
				{
					var waterTile = scene.WaterTiles[0];
					device.SetRenderTarget(waterRenderer.TargetRefraction);
					device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

					_context.ClipPlane = Mathematics.CreatePlane(
						waterTile.Height + 1.5f,
						-Vector3.Up,
						_context.ViewProjection,
						false);
					RefractionPass(scene);

					device.SetRenderTarget(waterRenderer.TargetReflection);
					device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

					var camera = scene.Camera;
					var distance = 2 * (camera.Position.Y - waterTile.Height);
					var oldPos = camera.Position;
					var pos = oldPos;
					pos.Y -= distance;
					camera.Position = pos;
					camera.PitchAngle = -camera.PitchAngle;
					_context.View = camera.View;

					_context.ClipPlane = Mathematics.CreatePlane(
						waterTile.Height - 0.5f,
						-Vector3.Up,
						_context.ViewProjection,
						true);

					ReflectionPass(scene);

					camera.Position = oldPos;
					camera.PitchAngle = -camera.PitchAngle;
					_context.View = camera.View;
				}
				finally
				{
					device.SetRenderTarget(null);
					device.Viewport = oldViewport;
				}
			}

			_context.ClipPlane = null;

			ReflectionPass(scene);

			if (scene.WaterTiles.Count > 0)
			{
				var waterRenderer = WaterRenderer;
				WaterRenderer.DrawWater(_context);
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