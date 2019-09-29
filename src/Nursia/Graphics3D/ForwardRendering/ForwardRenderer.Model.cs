using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;

namespace Nursia.Graphics3D.ForwardRendering
{
	partial class ForwardRenderer
	{
		internal void DrawMeshPart(MeshPart part)
		{
			if (part.Mesh == null ||
				part.Mesh.VertexBuffer == null ||
				part.Mesh.IndexBuffer == null ||
				part.Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var lights = _context.Lights;

			// Apply the effect and render items
			var effect = Assets.GetDefaultEffect(
				_context.ClipPlane != null,
				lights.Count > 0, 
				(int)part.BonesPerMesh);

			var worldViewProj = part.Transform * _context.World * _context.ViewProjection;

			if (part.BonesPerMesh != BonesPerMesh.None)
			{
				var boneTransforms = part.CalculateBoneTransforms();
				effect.Parameters["_bones"].SetValue(boneTransforms);
			}

			effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(part.Material.DiffuseColor.ToVector4());

			if (part.Material.Texture != null)
			{
				effect.Parameters["_texture"].SetValue(part.Material.Texture);
			}

			if (_context.ClipPlane != null)
			{
				var v = _context.ClipPlane.Value;
				effect.Parameters["_clipPlane"].SetValue(new Vector4(v.Normal, v.D));
			}

			device.Apply(part.Mesh);

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

					device.DrawIndexedPrimitives(effect, part.Mesh, part.StartVertex, part.VertexCount, part.StartIndex, part.PrimitiveCount);
				}

				if (lights.Count > 1)
				{
					// Change blend state back
					device.BlendState = BlendState;
				}
			}
			else
			{
				device.DrawIndexedPrimitives(effect, part.Mesh, part.StartVertex, part.VertexCount, part.StartIndex, part.PrimitiveCount);
			}

			++_context.Statistics.MeshesDrawn;
		}
	}
}
