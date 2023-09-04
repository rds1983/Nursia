using System.Linq;
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

			if (_context.HasLights)
			{
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				if (_context.DirectLight != null)
				{
					effect.Parameters["_dirLightColor"].SetValue(_context.DirectLight.Color.ToVector3());
					effect.Parameters["_dirLightDirection"].SetValue(_context.DirectLight.Direction);
				}

				if (_context.PointLights.Count > 0)
				{
					var pointLightColors = (from l in _context.PointLights select l.Color.ToVector3()).ToArray();
					effect.Parameters["_pointLightColor"].SetValue(pointLightColors);

					var pointLightPositions = (from l in _context.PointLights select l.Position).ToArray();
					effect.Parameters["_pointLightPosition"].SetValue(pointLightPositions);
				}
			}

			device.DrawIndexedPrimitives(effect, mesh.MeshData);

			++_context.Statistics.MeshesDrawn;
		}

		internal void DrawTerrain(Effect effect, TerrainTile tile)
		{
			var device = Nrs.GraphicsDevice;
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

			if (_context.HasLights)
			{
				var worldTransform = tile.Transform;
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				if (_context.DirectLight != null)
				{
					effect.Parameters["_dirLightColor"].SetValue(_context.DirectLight.Color.ToVector3());
					effect.Parameters["_dirLightDirection"].SetValue(_context.DirectLight.Direction);
				}
			}

			device.DrawIndexedPrimitives(effect, tile.MeshData);

			++_context.Statistics.MeshesDrawn;
		}
	}
}