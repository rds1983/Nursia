using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Lights
{
	public class BaseLight: ItemWithId
	{
		public Color Color { get; set; }
		public Vector3 Position { get; set; }
	}
}
