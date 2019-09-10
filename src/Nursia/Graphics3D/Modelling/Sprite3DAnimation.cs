using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class Sprite3DAnimation: ItemWithId
	{
		private readonly List<NodeAnimation> _boneAnimations = new List<NodeAnimation>();

		public List<NodeAnimation> BoneAnimations
		{
			get
			{
				return _boneAnimations;
			}
		}
	}
}
