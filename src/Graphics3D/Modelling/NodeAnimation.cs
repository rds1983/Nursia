using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class NodeAnimation
	{
		private readonly List<AnimationKeyframe> _frames = new List<AnimationKeyframe>();

		public ModelNode Node { get; set; }
		public List<AnimationKeyframe> Frames
		{
			get
			{
				return _frames;
			}
		}
	}
}
