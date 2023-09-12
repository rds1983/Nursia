using System;
using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	public class NodeInstance
	{
		private Matrix[] _boneTransforms = null;

		public ModelInstance Model { get; }
		public ModelNode Node { get; }

		public Matrix Transform { get; set; } = Matrix.Identity;

		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;
		public BoundingBox BoundingBox { get; internal set; }

		public bool HasSkin => Node.Skin != null;

		internal NodeInstance(ModelInstance model, ModelNode node)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));
			Node = node ?? throw new ArgumentNullException(nameof(node));
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null || _boneTransforms.Length != Node.Skin.JointIndices.Count)
			{
				_boneTransforms = new Matrix[Node.Skin.JointIndices.Count];
			}

			for (var i = 0; i < Node.Skin.JointIndices.Count; ++i)
			{
				var joint = Model.AllNodes[Node.Skin.JointIndices[i]];
				_boneTransforms[i] = Node.Skin.Transforms[i] * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}
