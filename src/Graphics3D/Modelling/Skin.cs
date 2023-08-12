using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	public class Skin
	{
		private Matrix[] _boneTransforms = null;

		public List<ModelNode> Joints { get; } = new List<ModelNode>();
		public Matrix[] Transforms { get; set; }

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null || _boneTransforms.Length != Joints.Count)
			{
				_boneTransforms = new Matrix[Joints.Count];
			}

			for (var i = 0; i < Joints.Count; ++i)
			{
				var joint = Joints[i];

				_boneTransforms[i] = Transforms[i] * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}