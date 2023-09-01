using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class ModelNode : ItemWithId
	{
		public List<Mesh> Meshes { get; } = new List<Mesh>();

		public Skin Skin { get; set; }

		public BoundingSphere BoundingSphere { get; set; }

		public ModelNode Parent { get; set; }

		public Vector3 DefaultTranslation { get; set; } = Vector3.Zero;
		public Vector3 DefaultScale { get; set; } = Vector3.One;
		public Quaternion DefaultRotation { get; set; } = Quaternion.Identity;

		public Matrix Transform { get; set; } = Matrix.Identity;

		internal Matrix DefaultAbsoluteTransform { get; set; }
		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;

		public List<ModelNode> Children { get; } = new List<ModelNode>();

		internal ModelNode FindNodeById(string id)
		{
			if (Id == id)
			{
				return this;
			}

			foreach (var child in Children)
			{
				var result = child.FindNodeById(id);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		internal void UpdateAbsoluteTransforms(Matrix rootTransform)
		{
			AbsoluteTransform = Transform * rootTransform;
			foreach (var child in Children)
			{
				child.UpdateAbsoluteTransforms(AbsoluteTransform);
			}
		}
	}
}
