using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class Sprite3DAnimation: ItemWithId
	{
		private readonly List<BoneAnimation> _boneAnimations = new List<BoneAnimation>();

		public List<BoneAnimation> BoneAnimations
		{
			get
			{
				return _boneAnimations;
			}
		}
	}
}
