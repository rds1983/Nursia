using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.Scene
{
	public class MeshNodePart
	{
		public Mesh Mesh { get; set; }
		public Material Material { get; set; }

		public void Draw(RenderContext context)
		{
			if (Mesh == null || Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			// Set the View matrix which defines the camera and what it's looking at
			var camera = context.Camera;
			camera.Viewport = new Vector2(device.Viewport.Width, device.Viewport.Height);
			var viewProjection = camera.View * camera.Projection;

			var lights = context.Lights;

			// Apply the effect and render items
			var effect = Assets.GetDefaultEffect(!Material.IgnoreLight, Material.Texture != null);

			device.SetVertexBuffer(Mesh.VertexBuffer);
			device.Indices = Mesh.IndexBuffer;

			var transform = context.Transform;
			var worldViewProj = transform * viewProjection;
			var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(transform));

			effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(Material.DiffuseColor.ToVector4());

			if (Material.Texture != null)
			{
				effect.Parameters["_texture"].SetValue(Material.Texture);
			}

			if (!Material.IgnoreLight)
			{
				effect.Parameters["_world"].SetValue(transform);
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				if (lights != null)
				{
					device.BlendState = BlendState.AlphaBlend;
					for (var i = 0; i < lights.Count; ++i)
					{
						if (i == 1)
						{
							device.BlendState = BlendState.Additive;
						}

						var dl = lights[i];

						effect.Parameters["_lightDir"].SetValue(dl.NormalizedDirection);
						effect.Parameters["_lightColor"].SetValue(dl.Color.ToVector3());

						foreach (var pass in effect.CurrentTechnique.Passes)
						{
							pass.Apply();
							device.DrawIndexedPrimitives(Mesh.PrimitiveType, 0, 0, Mesh.PrimitiveCount);
						}
					}
				}
			}
			else
			{
				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();

					device.DrawIndexedPrimitives(Mesh.PrimitiveType, 0, 0, Mesh.PrimitiveCount);
				}
			}
		}
	}
}
