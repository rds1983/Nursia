using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Primitives;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Modelling
{
	public class ModelNode : ItemWithId
	{
		private Matrix[] _boneTransforms = null;

		public ModelInstance Model { get; }
		public ModelNode Parent { get; internal set; }
		public List<ModelNode> Children { get; } = new List<ModelNode>();

		public List<ModelMesh> Meshes { get; } = new List<ModelMesh>();

		public Skin Skin { get; set; }

		public Vector3 DefaultTranslation { get; set; } = Vector3.Zero;
		public Vector3 DefaultScale { get; set; } = Vector3.One;
		public Quaternion DefaultRotation { get; set; } = Quaternion.Identity;

		public Matrix Transform { get; set; } = Matrix.Identity;

		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;
		public BoundingBox BoundingBox { get; internal set; }

		public bool HasSkin => Skin != null;

		public ModelNode(ModelInstance model)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null || _boneTransforms.Length != Skin.JointNodes.Count)
			{
				_boneTransforms = new Matrix[Skin.JointNodes.Count];
			}

			for (var i = 0; i < Skin.JointNodes.Count; ++i)
			{
				var joint = Skin.JointNodes[i];
				_boneTransforms[i] = Skin.Transforms[i] * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}

		internal void Render(NursiaModel node, RenderContext context, ref Matrix rootTransform)
		{
			if (Meshes.Count > 0)
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
					bonesTransforms = CalculateBoneTransforms();
				}

				foreach (var mesh in Meshes)
				{
					var m = mesh.Transform * AbsoluteTransform * rootTransform;
					var boundingBox = mesh.BoundingBox.Transform(ref m);
					if (context.Frustum.Contains(boundingBox) == ContainmentType.Disjoint)
					{
						//						continue;
					}

					if (HasSkin)
					{
						var asSkinnedMaterial = mesh.Material as ISkinnedMaterial;
						if (asSkinnedMaterial != null)
						{
							asSkinnedMaterial.BonesTransforms = bonesTransforms;
						}
					}

					context.BatchJob(node, mesh.Material, mesh.Transform * meshTransform, mesh.Mesh);
				}
			}

			foreach (var child in Children)
			{
				child.Render(node, context, ref rootTransform);
			}
		}
	}
}
