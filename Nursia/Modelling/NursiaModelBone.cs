using Microsoft.Xna.Framework;
using Nursia.Rendering;

namespace Nursia.Modelling
{
	public class NursiaModelBone
	{
		private NursiaModelBone[] _children;
		private NursiaModelMesh[] _meshes;

		public int Index { get; }
		public string Name { get; }
		public NursiaModelBone Parent { get; private set; }

		public NursiaModelBone[] Children
		{
			get => _children;

			internal set
			{
				if (value != null)
				{
					foreach (var b in value)
					{
						b.Parent = this;
					}
				}

				_children = value;
			}

		}

		public NursiaModelMesh[] Meshes
		{
			get => _meshes;

			internal set
			{
				if (value != null)
				{
					foreach (var m in value)
					{
						m.ParentBone = this;
					}
				}

				_meshes = value;
			}
		}


		public Skin Skin { get; set; }

		public Vector3 DefaultTranslation = Vector3.Zero;
		public Vector3 DefaultScale = Vector3.One;
		public Quaternion DefaultRotation = Quaternion.Identity;

		public Matrix Transform = Matrix.Identity;

		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;
		public BoundingBox BoundingBox { get; internal set; }

		public bool HasSkin => Skin != null;

		internal NursiaModelBone(int index, string name)
		{
			Index = index;
			Name = name;
		}

		internal void Render(RenderBatch batch, ref Matrix rootTransform)
		{
			if (Meshes.Length > 0)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix meshTransform = HasSkin ? rootTransform : AbsoluteTransform * rootTransform;
				Matrix[] bonesTransforms = null;

				// Apply the effect and render items
				if (HasSkin)
				{
					bonesTransforms = Skin.CalculateBoneTransforms();
				}

				foreach (var mesh in Meshes)
				{
					if (HasSkin)
					{
						mesh.Mesh.BonesTransforms = bonesTransforms;
					}

					batch.BatchJob(mesh.Material, mesh.Transform * meshTransform, mesh.Mesh);
				}
			}

			foreach (var child in Children)
			{
				child.Render(batch, ref rootTransform);
			}
		}
	}
}
