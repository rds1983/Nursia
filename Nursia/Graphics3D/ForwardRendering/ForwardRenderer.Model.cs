using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Landscape;

namespace Nursia.Graphics3D.ForwardRendering
{
	partial class ForwardRenderer
	{
		internal void DrawMesh(Effect effect, Mesh mesh, ref Matrix worldTransform)
		{
			if (mesh == null || mesh.VertexBuffer == null || mesh.IndexBuffer == null || mesh.Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var lights = _context.Lights;

			var worldViewProj = mesh.Transform * worldTransform * _context.ViewProjection;

			effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(mesh.Material.DiffuseColor.ToVector4());

			if (mesh.Material.Texture != null)
			{
				effect.Parameters["_texture"].SetValue(mesh.Material.Texture);
			}

			if (_context.ClipPlane != null)
			{
				var v = _context.ClipPlane.Value;
				effect.Parameters["_clipPlane"].SetValue(new Vector4(v.Normal, v.D));
			}

			device.Apply(mesh.MeshData);

			if (lights.Count > 0)
			{
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				for (var i = 0; i < lights.Count; ++i)
				{
					if (i == 1)
					{
						device.BlendState = BlendState.Additive;
					}

					var dl = lights[i];

					effect.Parameters["_lightDir"].SetValue(dl.NormalizedDirection);
					effect.Parameters["_lightColor"].SetValue(dl.Color.ToVector3());

					device.DrawIndexedPrimitives(effect, mesh.MeshData);
				}

				if (lights.Count > 1)
				{
					// Change blend state back
					device.BlendState = BlendState;
				}
			}
			else
			{
				device.DrawIndexedPrimitives(effect, mesh.MeshData);
			}

			++_context.Statistics.MeshesDrawn;
		}

		internal void DrawTerrain(Effect effect, TerrainTile tile)
		{
			var device = Nrs.GraphicsDevice;
			var lights = _context.Lights;
			var worldViewProj = tile.Transform * _context.ViewProjection;

			effect.Parameters["_worldViewProjection"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(tile.Terrain.DiffuseColor.ToVector4());
			effect.Parameters["_textureBase"].SetValue(tile.Terrain.TextureBase);

			if (_context.ClipPlane != null)
			{
				var v = _context.ClipPlane.Value;
				effect.Parameters["_clipPlane"].SetValue(new Vector4(v.Normal, v.D));
			}

			device.Apply(tile.MeshData);

			if (lights.Count > 0)
			{
				var worldTransform = tile.Transform;
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				for (var i = 0; i < lights.Count; ++i)
				{
					if (i == 1)
					{
						device.BlendState = BlendState.Additive;
					}

					var dl = lights[i];

					effect.Parameters["_lightDir"].SetValue(dl.NormalizedDirection);
					effect.Parameters["_lightColor"].SetValue(dl.Color.ToVector3());

					device.DrawIndexedPrimitives(effect, tile.MeshData);
				}

				if (lights.Count > 1)
				{
					// Change blend state back
					device.BlendState = BlendState;
				}
			}
			else
			{
				device.DrawIndexedPrimitives(effect, tile.MeshData);
			}

			++_context.Statistics.MeshesDrawn;
		}
	}
}