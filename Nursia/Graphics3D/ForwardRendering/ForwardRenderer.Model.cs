using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.ForwardRendering
{
	partial class ForwardRenderer
	{
		internal void DrawMesh(Effect effect, Mesh mesh)
		{
			if (mesh == null || mesh.VertexBuffer == null || mesh.IndexBuffer == null || mesh.Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var lights = _context.Lights;

			var worldViewProj = mesh.Transform * _context.World * _context.ViewProjection;

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
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(_context.World));
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

			if (Nrs.DrawBoundingBoxes)
			{
				device.RasterizerState = RasterizerState.CullNone;
				device.RasterizerState.FillMode = FillMode.WireFrame;
				var colorEffect = Assets.ColorEffect;

				var boundingBoxTransform = Matrix.CreateTranslation(Vector3.One) *
					Matrix.CreateScale((mesh.BoundingBox.Max.X - mesh.BoundingBox.Min.X) / 2.0f,
					(mesh.BoundingBox.Max.Y - mesh.BoundingBox.Min.Y) / 2.0f,
					(mesh.BoundingBox.Max.Z - mesh.BoundingBox.Min.Z) / 2.0f) *
					Matrix.CreateTranslation(mesh.BoundingBox.Min);

				colorEffect.Parameters["_transform"].SetValue(boundingBoxTransform * worldViewProj);
				colorEffect.Parameters["_color"].SetValue(Color.Green.ToVector4());

				device.Apply(PrimitiveMeshes.CubePosition);
				device.DrawIndexedPrimitives(colorEffect, PrimitiveMeshes.CubePosition);

				device.RasterizerState = RasterizerState;
			}

			++_context.Statistics.MeshesDrawn;
		}
	}
}