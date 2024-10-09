using System.Collections.Generic;

namespace Nursia.Animation
{
	public class AnimationClip : ItemWithId
	{
		public List<BoneAnimation> BoneAnimations { get; } = new List<BoneAnimation>();
		public float Time { get; set; }
	}
}
