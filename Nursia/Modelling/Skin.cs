using System;
using Microsoft.Xna.Framework;

namespace Nursia.Modelling
{
	public class Skin : ItemWithId
	{
		private readonly Matrix[] _boneTransforms;

		public NursiaModelBone[] JointNodes { get; }

		public Matrix[] InverseBindTransforms { get; }

		public Skin(NursiaModelBone[] jointNodes, Matrix[] inverseBindTransforms)
		{
			if (jointNodes == null)
			{
				throw new ArgumentNullException(nameof(jointNodes));
			}

			if (jointNodes.Length == 0)
			{
				throw new ArgumentException(nameof(jointNodes), "no joints");
			}

			if (inverseBindTransforms == null)
			{
				throw new ArgumentNullException(nameof(inverseBindTransforms));
			}

			if (inverseBindTransforms.Length == 0)
			{
				throw new ArgumentException(nameof(inverseBindTransforms), "no inverse bind transforms");
			}

			if (jointNodes.Length != inverseBindTransforms.Length)
			{
				throw new ArgumentException($"Different sizes. JointNodes has {jointNodes.Length}, InverseBindTransforms has {inverseBindTransforms.Length}");
			}

			JointNodes = jointNodes;
			InverseBindTransforms = inverseBindTransforms;
			_boneTransforms = new Matrix[jointNodes.Length];
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			for (var i = 0; i < JointNodes.Length; ++i)
			{
				var joint = JointNodes[i];
				_boneTransforms[i] = InverseBindTransforms[i] * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}