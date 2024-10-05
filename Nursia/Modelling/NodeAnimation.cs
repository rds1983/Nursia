using Microsoft.Xna.Framework;
using System;

namespace Nursia.Modelling
{
	public class NodeAnimation
	{
		public NursiaModelBone Node { get; }
		public AnimationTransforms<Vector3> Translations { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Vector3> Scales { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Quaternion> Rotations { get; } = new AnimationTransformsQuaternion();

		public NodeAnimation(NursiaModelBone node)
		{
			Node = node ?? throw new ArgumentNullException(nameof(node));
		}
	}
}
