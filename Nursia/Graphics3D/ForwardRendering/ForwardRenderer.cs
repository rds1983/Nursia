using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		private RenderTargetUsage _oldRenderTargetUsage;
		private bool _beginCalled;
		private readonly RenderContext _context = new RenderContext();
		private WaterRenderer _waterRenderer;

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullClockwise;
		public BlendState BlendState { get; set; } = BlendState.AlphaBlend;
		public SamplerState SamplerState { get; set; } = SamplerState.LinearWrap;

		public RenderStatistics Statistics => _context.Statistics;

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

		public RenderTarget2D WaterReflection => WaterRenderer.TargetReflection;

		public RenderTarget2D WaterRefraction => WaterRenderer.TargetRefraction;

		public Matrix Projection => _context.Projection;
		public Matrix View => _context.View;

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
			_oldRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;

			device.BlendState = BlendState;
			device.DepthStencilState = DepthStencilState;
			device.RasterizerState = RasterizerState;
			device.SamplerStates[0] = SamplerState;
			device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

			_beginCalled = true;

			_context.Statistics.Reset();
		}

		private void DrawMeshNode(ModelNode meshNode, ref Matrix rootTransform)
		{
			if (meshNode.Meshes.Count > 0)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix meshTransform = meshNode.Skin == null ? meshNode.AbsoluteTransform * rootTransform : rootTransform;
				Matrix[] boneTransforms = null;
				// Apply the effect and render items
				if (meshNode.Skin != null)
				{
					boneTransforms = meshNode.Skin.CalculateBoneTransforms();
				}

				foreach (var mesh in meshNode.Meshes)
				{
					var effect = Resources.GetDefaultEffect(
						mesh.Material.Texture != null,
						meshNode.Skin != null,
						_context.ClipPlane != null,
						_context.DirectLight != null && mesh.HasNormals,
						mesh.HasNormals ? _context.PointLights.Count : 0);
					if (meshNode.Skin != null)
					{
						effect.Parameters["_bones"].SetValue(boneTransforms);
					}

					var m = mesh.Transform * meshNode.AbsoluteTransform;
					var boundingBox = mesh.BoundingBox.Transform(ref m);
					if (_context.Frustrum.Contains(boundingBox) == ContainmentType.Disjoint)
					{
						continue;
					}

					DrawMesh(effect, mesh, ref meshTransform);

					if (Nrs.DrawBoundingBoxes)
					{
						var device = Nrs.GraphicsDevice;
						device.RasterizerState = RasterizerState.CullNone;
						device.RasterizerState.FillMode = FillMode.WireFrame;
						var colorEffect = Resources.ColorEffect;

						var boundingBoxTransform = Matrix.CreateTranslation(Vector3.One) *
							Matrix.CreateScale((mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X) / 2.0f,
							(mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y) / 2.0f,
							(mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z) / 2.0f) *
							Matrix.CreateTranslation(mesh.BoundingBox.Min);

						colorEffect.Parameters["_transform"].SetValue(boundingBoxTransform * m * _context.ViewProjection);
						colorEffect.Parameters["_color"].SetValue(Color.Green.ToVector4());

						device.Apply(PrimitiveMeshes.CubePosition);
						device.DrawIndexedPrimitives(colorEffect, PrimitiveMeshes.CubePosition);

						device.RasterizerState = RasterizerState;
					}
				}
			}

			foreach (var child in meshNode.Children)
			{
				DrawMeshNode(child, ref rootTransform);
			}
		}

		private void DrawModel(NursiaModel model)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			model.UpdateNodesAbsoluteTransforms();
			foreach (var mesh in model.Nodes)
			{
				DrawMeshNode(mesh, ref model.Transform);
			}
		}

		private void RefractionPass(Scene scene)
		{
			if (scene.Terrain != null)
			{
				var effect = Resources.GetTerrainEffect(0, _context.ClipPlane != null, scene.HasMarker, _context.DirectLight != null);
				if (scene.HasMarker)
				{
					var markerPosition = scene.Marker.Position.Value;
					effect.Parameters["_markerPosition"].SetValue(markerPosition);
					effect.Parameters["_markerRadius"].SetValue(scene.Marker.Radius);
				}

				for (var x = 0; x < scene.Terrain.TilesPerX; ++x)
				{
					for (var z = 0; z < scene.Terrain.TilesPerZ; ++z)
					{
						var terrainTile = scene.Terrain[x, z];

						var m = terrainTile.Transform;
						var boundingBox = terrainTile.MeshData.BoundingBox.Transform(ref m);
						if (_context.Frustrum.Contains(boundingBox) == ContainmentType.Disjoint)
						{
							continue;
						}

						if (scene.HasMarker)
						{
							effect.Parameters["_world"].SetValue(terrainTile.Transform);
						}
						DrawTerrain(effect, terrainTile);
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
				var effect = Resources.SkyboxEffect;

				var view = _context.View;
				view.Translation = Vector3.Zero;
				var transform = view * _context.Projection;

				effect.Parameters["_transform"].SetValue(skybox.Transform * transform);
				effect.Parameters["_texture"].SetValue(skybox.Texture);

				device.Apply(skybox.MeshData);

				device.SamplerStates[0] = SamplerState.LinearClamp;
				device.DrawIndexedPrimitives(effect, skybox.MeshData);
				device.SamplerStates[0] = SamplerState;

				++_context.Statistics.MeshesDrawn;

				device.DepthStencilState = DepthStencilState;
			}

			RefractionPass(scene);
		}

		public void DrawScene(Scene scene)
		{
			if (Nrs.GraphicsDevice.Viewport.Width == 0 || Nrs.GraphicsDevice.Viewport.Height == 0)
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
			device.PresentationParameters.RenderTargetUsage = _oldRenderTargetUsage;


			_beginCalled = false;
		}
	}
}