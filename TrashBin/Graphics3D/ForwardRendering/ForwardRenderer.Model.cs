using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Landscape;

namespace Nursia.Graphics3D.ForwardRendering
{
	partial class ForwardRenderer
	{
		private void DrawMesh(Effect effect, Mesh mesh, ref Matrix worldTransform)
		{
			if (mesh == null || mesh.VertexBuffer == null || mesh.IndexBuffer == null || mesh.Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var world = mesh.Transform * worldTransform;
			var worldViewProj = worldTransform * _context.ViewProjection;

			effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(mesh.Material.DiffuseColor.ToVector4());
			effect.Parameters["_world"].SetValue(world);
			effect.Parameters["_cameraPosition"].SetValue(_context.Scene.Camera.Position);

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

				_context.SetLights(effect);
				effect.Parameters["_specularPower"].SetValue(mesh.Material.SpecularPower);
				effect.Parameters["_specularFactor"].SetValue(mesh.Material.SpecularFactor);
			}

			switch (_context.RenderPassType)
			{
				case RenderPassType.Color:
					effect.CurrentTechnique = effect.Techniques["Default"];
					break;
				case RenderPassType.Depth:
					effect.CurrentTechnique = effect.Techniques["Depth"];
					break;
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
			effect.Parameters["_world"].SetValue(tile.Transform);
			effect.Parameters["_cameraPosition"].SetValue(_context.Scene.Camera.Position);

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

				_context.SetLights(effect);
			}

			switch (_context.RenderPassType)
			{
				case RenderPassType.Color:
					effect.CurrentTechnique = effect.Techniques["Color"];
					break;
				case RenderPassType.Depth:
					effect.CurrentTechnique = effect.Techniques["Depth"];
					break;
			}

			device.DrawIndexedPrimitives(effect, tile.MeshData);

			++_context.Statistics.MeshesDrawn;
		}
	}
}