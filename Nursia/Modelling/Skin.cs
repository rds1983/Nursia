using System;
using Microsoft.Xna.Framework;

namespace Nursia.Modelling
{
	public class Skin : ItemWithId
	{
		private readonly Matrix[] _boneTransforms;

		public NursiaModelBone[] Bones { get; }

		public Matrix[] InverseBindTransforms { get; }

		public Skin(NursiaModelBone[] bones, Matrix[] inverseBindTransforms)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no joints");
			}

			if (inverseBindTransforms == null)
			{
				throw new ArgumentNullException(nameof(inverseBindTransforms));
			}

			if (inverseBindTransforms.Length == 0)
			{
				throw new ArgumentException(nameof(inverseBindTransforms), "no inverse bind transforms");
			}

			if (bones.Length != inverseBindTransforms.Length)
			{
				throw new ArgumentException($"Different sizes. JointNodes has {bones.Length}, InverseBindTransforms has {inverseBindTransforms.Length}");
			}

			Bones = bones;
			InverseBindTransforms = inverseBindTransforms;
			_boneTransforms = new Matrix[bones.Length];
		}

		public Skin(NursiaModelBone[] bones)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no joints");
			}

			Bones = bones;

			// Automatically calculate inverse bind transform
			InverseBindTransforms = new Matrix[bones.Length];
			for(var i = 0; i < bones.Length; ++i)
			{
				var bone = bones[i];
				InverseBindTransforms[i] = Matrix.Invert(bone.AbsoluteTransform);
			}

			_boneTransforms = new Matrix[bones.Length];
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			for (var i = 0; i < Bones.Length; ++i)
			{
				var joint = Bones[i];

				_boneTransforms[i] = InverseBindTransforms[i] * joint.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}