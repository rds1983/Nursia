using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Scene
{
	public class Bone: ItemWithId
	{
		public Matrix Transform;
		public int Index { get; set; }
		public BoneNode ParentNode { get; set; }

		public Bone()
		{
			Transform = Matrix.Identity;
		}
	}
}
