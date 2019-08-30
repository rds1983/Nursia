using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Scene
{
	public class Bone
	{
		public Matrix Transform;
		public int Index { get; set; }
		public Node ParentNode { get; set; }
		internal string NodeId { get; set; }

		public Bone()
		{
			Transform = Matrix.Identity;
		}
	}
}
