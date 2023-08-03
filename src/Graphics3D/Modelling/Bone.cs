using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	public class Bone
	{
		public Matrix Transform;
		public ModelNode ParentNode { get; set; }
		internal string NodeId { get; set; }

		public Bone()
		{
			Transform = Matrix.Identity;
		}
	}
}
