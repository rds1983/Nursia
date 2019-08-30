using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class BoneAnimation
	{
		private readonly List<AnimationKeyframe> _frames = new List<AnimationKeyframe>();

		public Node Node { get; set; }
		public List<AnimationKeyframe> Frames
		{
			get
			{
				return _frames;
			}
		}
	}
}
