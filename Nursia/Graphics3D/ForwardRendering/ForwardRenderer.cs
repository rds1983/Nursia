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
		private RenderTargetUsage _oldRenderTargetUsage;
		private bool _beginCalled;
		private readonly RenderContext _context = new RenderContext();
		private WaterRenderer _waterRenderer;

		public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
		public RasterizerState RasterizerState { get; set; } = RasterizerState.CullClockwise;
		public BlendState BlendState { get; set; } = BlendState.AlphaBlend;
		
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
			_oldRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;

			device.BlendState = BlendState;
			device.DepthStencilState = DepthStencilState;
			device.RasterizerState = RasterizerState;

			device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

			_beginCalled = true;

			_context.Statistics.Reset();
		}

		private void DrawMeshNode(NodeInstance node, ref Matrix rootTransform)
		{
			if (node.Node.Meshes.Count > 0)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix meshTransform = node.HasSkin ? rootTransform : node.AbsoluteTransform * rootTransform;
				Matrix[] boneTransforms = null;
				// Apply the effect and render items
				if (node.HasSkin)
				{
					boneTransforms = node.CalculateBoneTransforms();
				}

				foreach (var mesh in node.Node.Meshes)
				{
					var effect = Resources.GetDefaultEffect(
						mesh.Material.Texture != null,
						node.HasSkin,
						_context.ClipPlane != null,
						_context.HasLights && mesh.HasNormals);

					if (node.HasSkin)
					{
						effect.Parameters["_bones"].SetValue(boneTransforms);
					}

					var m = mesh.Transform * node.AbsoluteTransform * rootTransform;
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

						var boundingBoxTransform = Matrix.CreateScale((mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X),
							(mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y),
							(mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z)) *
							Matrix.CreateTranslation(mesh.BoundingBox.Min);

						colorEffect.Parameters["_transform"].SetValue(boundingBoxTransform * m * _context.ViewProjection);
						colorEffect.Parameters["_color"].SetValue(Color.Green.ToVector4());

						device.Apply(PrimitiveMeshes.CubePositionFromZeroToOne);
						device.DrawIndexedPrimitives(colorEffect, PrimitiveMeshes.CubePositionFromZeroToOne);

						device.RasterizerState = RasterizerState;
					}
				}
			}

			foreach (var childIndex in node.Node.ChildrenIndices)
			{
				DrawMeshNode(node.Model.AllNodes[childIndex], ref rootTransform);
			}
		}

		private void DrawModel(ModelInstance model)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			model.UpdateNodesAbsoluteTransforms();
			DrawMeshNode(model.RootNode, ref model.Transform);
		}

		private void RefractionPass(Scene scene)
		{
			if (scene.Terrain != null)
			{
				var terrain = scene.Terrain;
				var effect = Resources.GetTerrainEffect(terrain.TexturesCount - 1, _context.ClipPlane != null,
														scene.HasMarker, _context.HasLights);
				if (scene.HasMarker)
				{
					var markerPosition = scene.Marker.Position.Value;
					effect.Parameters["_markerPosition"].SetValue(markerPosition);
					effect.Parameters["_markerRadius"].SetValue(scene.Marker.Radius);
				}

				if (terrain.TexturePaint1 != null)
				{
					effect.Parameters["_texture1"].SetValue(terrain.TexturePaint1);
				}

				if (terrain.TexturePaint2 != null)
				{
					effect.Parameters["_texture2"].SetValue(terrain.TexturePaint2);
				}

				if (terrain.TexturePaint3 != null)
				{
					effect.Parameters["_texture3"].SetValue(terrain.TexturePaint3);
				}

				if (terrain.TexturePaint4 != null)
				{
					effect.Parameters["_texture4"].SetValue(terrain.TexturePaint4);
				}

				effect.Parameters["_textureScaleX"].SetValue(terrain.TileTextureScale.X);
				effect.Parameters["_textureScaleY"].SetValue(terrain.TileTextureScale.Y);

				for (var x = 0; x < terrain.TilesCount.X; ++x)
				{
					for (var y = 0; y < terrain.TilesCount.Y; ++y)
					{
						var terrainTile = terrain[x, y];

						var m = terrainTile.Transform;
						var boundingBox = terrainTile.MeshData.BoundingBox.Transform(ref m);
						if (_context.Frustrum.Contains(boundingBox) == ContainmentType.Disjoint)
						{
							continue;
						}

						if (scene.HasMarker || scene.HasLights)
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

				device.DrawIndexedPrimitives(effect, skybox.MeshData);

				++_context.Statistics.MeshesDrawn;

				device.DepthStencilState = DepthStencilState;
			}

			RefractionPass(scene);
		}

		private bool PrepareDraw(Camera camera)
		{
			if (Nrs.GraphicsDevice.Viewport.Width == 0 || Nrs.GraphicsDevice.Viewport.Height == 0)
			{
				return false;
			}

			_context.View = camera.View;
			_context.Projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(camera.ViewAngle),
				Nrs.GraphicsDevice.Viewport.AspectRatio,
				NearPlaneDistance, FarPlaneDistance);

			return true;
		}

		public void DrawMesh(Mesh mesh, Camera camera)
		{
			if (!PrepareDraw(camera))
			{
				return;
			}

			var effect = Resources.GetDefaultEffect(
					mesh.Material.Texture != null,
					false,
					_context.ClipPlane != null,
					_context.HasLights && mesh.HasNormals);

			var m = Matrix.Identity;
			DrawMesh(effect, mesh, ref m);
		}

		public void DrawModel(ModelInstance model, Camera camera)
		{
			if (!PrepareDraw(camera))
			{
				return;
			}

			DrawModel(model);
		}

		public void DrawScene(Scene scene)
		{
			if (!PrepareDraw(scene.Camera))
			{
				return;
			}

			_context.Scene = scene;

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
			device.PresentationParameters.RenderTargetUsage = _oldRenderTargetUsage;

			_beginCalled = false;
		}
	}
}