using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nursia.Modelling
{
	public class Skin : ItemWithId
	{
		private Matrix[] _boneTransforms = null;

		public List<NursiaModelBone> JointNodes { get; } = new List<NursiaModelBone>();

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null || _boneTransforms.Length != JointNodes.Count)
			{
				_boneTransforms = new Matrix[JointNodes.Count];
			}

			for (var i = 0; i < JointNodes.Count; ++i)
			{
				var joint = JointNodes[i];
				_boneTransforms[i] = joint.InverseBindTransform * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}