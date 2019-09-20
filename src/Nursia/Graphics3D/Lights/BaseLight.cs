using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Lights
{
	public abstract class BaseLight: ItemWithId
	{
		public Color Color { get; set; }
		public Vector3 Position { get; set; }

		protected BaseLight()
		{
			Color = Color.White;
		}
	}
}
