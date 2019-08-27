using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class BoneNode: ItemWithId
	{
		private readonly List<BoneNode> _children = new List<BoneNode>();

		public Matrix Transform;

		internal Matrix AbsoluteTransform
		{
			get; set;
		}

		public BoneNode Parent { get; set; }

		public List<BoneNode> Children
		{
			get
			{
				return _children;
			}
		}

		public BoneNode()
		{
			Transform = Matrix.Identity;
		}
	}
}
