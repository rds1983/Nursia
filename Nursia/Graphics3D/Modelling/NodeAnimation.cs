using System;
using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	public class NodeAnimation
	{
		public int NodeIndex { get; }
		public AnimationTransforms<Vector3> Translations { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Vector3> Scales { get; } = new AnimationTransformsVector3();
		public AnimationTransforms<Quaternion> Rotations { get; } = new AnimationTransformsQuaternion();

		public NodeAnimation(int nodeIndex)
		{
			NodeIndex = nodeIndex;
		}
	}
}
