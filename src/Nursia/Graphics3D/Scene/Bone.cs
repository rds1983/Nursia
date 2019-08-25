using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class Bone: ItemWithId
	{
		private readonly List<Bone> _children = new List<Bone>();

		public Matrix Transform { get; set; }

		public Matrix AbsoluteTransform
		{
			get
			{
				if (Parent != null)
				{
					return Transform * Parent.AbsoluteTransform;
				}

				return Transform;
			}
		}

		public Bone Parent { get; set; }

		public List<Bone> Children
		{
			get
			{
				return _children;
			}
		}

		public Bone()
		{
			Transform = Matrix.Identity;
		}
	}
}
