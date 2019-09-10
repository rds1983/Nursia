using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class MeshPart
	{
		private readonly List<Bone> _bones = new List<Bone>();
		private Matrix[] _boneTransforms = null;

		internal string MeshPartId { get; set; }
		internal string MaterialId { get; set; }

		public Material Material { get; set; }
		public Mesh Mesh { get; set; }

		public BonesPerMesh BonesPerMesh { get; set; }

		public BoundingSphere BoundingSphere { get; set; }

		public List<Bone> Bones
		{
			get
			{
				return _bones;
			}
		}

		private Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null ||
				_boneTransforms.Length != Bones.Count)
			{
				_boneTransforms = new Matrix[Bones.Count];
			}

			for (var i = 0; i < Bones.Count; ++i)
			{
				_boneTransforms[i] = Bones[i].Transform * Bones[i].ParentNode.AbsoluteTransform;
			}

			return _boneTransforms;
		}

		internal void Draw(RenderContext context)
		{
			if (Mesh == null ||
				Mesh.VertexBuffer == null ||
				Mesh.IndexBuffer == null || 
				Material == null)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;

			var lights = context.Lights;

			// Apply the effect and render items
			var effect = Assets.GetDefaultEffect(lights.Count > 0, (int)BonesPerMesh);


			device.SetVertexBuffer(Mesh.VertexBuffer);
			device.Indices = Mesh.IndexBuffer;

			var worldViewProj = context.World * context.ViewProjection;

			if (BonesPerMesh != BonesPerMesh.None)
			{
				var boneTransforms = CalculateBoneTransforms();
				effect.Parameters["_bones"].SetValue(boneTransforms);
			}

			effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
			effect.Parameters["_diffuseColor"].SetValue(Material.DiffuseColor.ToVector4());

			if (Material.Texture != null)
			{
				effect.Parameters["_texture"].SetValue(Material.Texture);
			}

			device.Apply(Mesh);

			if (lights.Count > 0)
			{
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(context.World));
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

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.DrawIndexedPrimitives(Mesh);
					}
				}

				if (lights.Count > 1)
				{
					// Change blend state back
					device.BlendState = BlendState.AlphaBlend;
				}
			}
			else
			{
				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawIndexedPrimitives(Mesh);
				}
			}

			++context.Statistics.MeshesDrawn;
		}
	}
}
