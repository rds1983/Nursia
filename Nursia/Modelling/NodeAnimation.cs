using Microsoft.Xna.Framework;
using System;

namespace Nursia.Modelling
{
	public class NodeAnimation
	{
		public NursiaModelBone Bone { get; }
		public AnimationTransforms<Vector3> Translations { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Vector3> Scales { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Quaternion> Rotations { get; } = new AnimationTransformsQuaternion();

		public NodeAnimation(NursiaModelBone bone)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
		}
	}
}
