using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.Scene
{
	public class MeshPart
	{
		internal string MaterialName { get; set; }

		public Material Material { get; set; }
		public VertexBuffer VertexBuffer { get; set; }
		public IndexBuffer IndexBuffer { get; set; }

		public BonesPerMesh BonesPerMesh { get; set; }

		public int PrimitiveCount
		{
			get
			{
				return IndexBuffer.IndexCount / 3;
			}
		}

		public void Draw(RenderContext context)
		{
			if (VertexBuffer == null || 
				IndexBuffer == null || 
				Material == null)
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
			var effect = Assets.GetDefaultEffect(!Material.IgnoreLight, (int)BonesPerMesh);

			effect.Parameters["_bones"].SetValue(context.BoneTransforms);

			device.SetVertexBuffer(VertexBuffer);
			device.Indices = IndexBuffer;

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
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);

				if (lights != null)
				{
					device.BlendState = BlendState.Opaque;
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
							device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 
								VertexBuffer.VertexCount, 0, PrimitiveCount);
						}
					}
				}
			}
			else
			{
				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();

					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
						VertexBuffer.VertexCount, 0, PrimitiveCount);
				}
			}
		}

		internal static MeshPart Create<T>(T[] vertices, short[] indices, PrimitiveType primitiveType) where T : struct, IVertexType
		{
			var vertexBuffer = new VertexBuffer(Nrs.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
													BufferUsage.None);
			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			indexBuffer.SetData(indices);

			return new MeshPart
			{
				VertexBuffer = vertexBuffer,
				IndexBuffer = indexBuffer
			};
		}
	}
}
