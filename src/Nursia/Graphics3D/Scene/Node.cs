using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Scene
{
	public abstract class Node: ItemWithId
	{
		public Matrix Transform;

		public Node()
		{
			Transform = Matrix.Identity;
		}

		public abstract void Draw(RenderContext context);
	}
}
