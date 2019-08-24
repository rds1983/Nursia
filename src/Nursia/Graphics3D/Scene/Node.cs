using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class Node: ItemWithId
	{
		private readonly List<Node> _children = new List<Node>();

		public Matrix Transform;

		public List<Node> Children
		{
			get
			{
				return _children;
			}
		}

		public Node()
		{
			Transform = Matrix.Identity;
		}

		public virtual void Draw(RenderContext context)
		{
			using (var scope = new TransformScope(context, Transform))
			{
				foreach (var child in Children)
				{
					child.Draw(context);
				}
			}
		}
	}
}
