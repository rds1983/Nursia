using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

using DirectionalLight = Nursia.Graphics3D.Lights.DirectionalLight;

namespace Nursia.Graphics3D
{
	public class Renderer
	{
		private readonly RenderContext renderContext = new RenderContext();

		public Camera Camera
		{
			get
			{
				return renderContext.Camera;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				renderContext.Camera = value;
			}
		}

		public List<DirectionalLight> Lights
		{
			get
			{
				return renderContext.Lights;
			}
		}

		public void Render(Sprite3D sprite)
		{
			var device = Nrs.GraphicsDevice;

			// Set the View matrix which defines the camera and what it's looking at
			var camera = Camera;
			camera.Viewport = new Vector2(device.Viewport.Width, device.Viewport.Height);
			var viewProjection = camera.View * camera.Projection;

			var lights = renderContext.Lights;

			// Apply the effect and render items
			foreach (var mesh in sprite.Meshes)
			{
				var material = mesh.Material;
				if (material == null)
				{
					continue;
				}

				var effect = Assets.GetDefaultEffect(!mesh.Material.IgnoreLight, mesh.Material.Texture != null);

				device.SetVertexBuffer(mesh.VertexBuffer);
				device.Indices = mesh.IndexBuffer;

				var worldViewProj = mesh.Transform * viewProjection;
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.Transform));

				effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
				effect.Parameters["_diffuseColor"].SetValue(material.DiffuseColor.ToVector4());

				if (mesh.Material.Texture != null)
				{
					effect.Parameters["_texture"].SetValue(material.Texture);
				}

				if (!material.IgnoreLight)
				{
					effect.Parameters["_world"].SetValue(mesh.Transform);
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
								device.DrawIndexedPrimitives(mesh.PrimitiveType, 0, 0, mesh.PrimitiveCount);
							}
						}
					}
				}
				else
				{
					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();

						device.DrawIndexedPrimitives(mesh.PrimitiveType, 0, 0, mesh.PrimitiveCount);
					}
				}
			}
		}
	}
}
