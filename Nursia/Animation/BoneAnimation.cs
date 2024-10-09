using Microsoft.Xna.Framework;
using Nursia.Modelling;
using System;

namespace Nursia.Animation
{
	public class BoneAnimation
	{
		public NursiaModelBone Bone { get; }
		public AnimationTransforms<Vector3> Translations { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Vector3> Scales { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Quaternion> Rotations { get; } = new AnimationTransformsQuaternion();

		public BoneAnimation(NursiaModelBone bone)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
		}
	}
}
