using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Landscape;

namespace Nursia.Graphics3D.ForwardRendering
{
	partial class ForwardRenderer
	{
		private int[] _effectLightType = new int[Constants.MaxLights];
		private Vector3[] _effectLightPosition = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightDirection = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightColor = new Vector3[Constants.MaxLights];

		private void SetLights(Effect effect)
		{
			var lightIndex = 0;
			foreach (var directLight in _context.DirectLights)
			{
				if (lightIndex >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[lightIndex] = 0;
				_effectLightColor[lightIndex] = directLight.Color.ToVector3();
				_effectLightDirection[lightIndex] = directLight.Direction;

				++lightIndex;
			}

			foreach (var pointLight in _context.PointLights)
			{
				if (lightIndex >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[lightIndex] = 1;
				_effectLightColor[lightIndex] = pointLight.Color.ToVector3();
				_effectLightPosition[lightIndex] = pointLight.Position;

				++lightIndex;
			}

			effect.Parameters["_lightType"].SetValue(_effectLightType);
			effect.Parameters["_lightPosition"].SetValue(_effectLightPosition);
			effect.Parameters["_lightDirection"].SetValue(_effectLightDirection);
			effect.Parameters["_lightColor"].SetValue(_effectLightColor);
			effect.Parameters["_lightCount"].SetValue(lightIndex);
		}

		private void DrawMesh(Effect effect, Mesh mesh, ref Matrix worldTransform)
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

			if (_context.HasLights && mesh.HasNormals)
			{
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				SetLights(effect);
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

			if (tile.Terrain.TexturesCount > 1)
			{
				effect.Parameters["_textureBlend"].SetValue(tile.SplatTexture);
			}

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

				SetLights(effect);
			}

			device.DrawIndexedPrimitives(effect, tile.MeshData);

			++_context.Statistics.MeshesDrawn;
		}
	}
}