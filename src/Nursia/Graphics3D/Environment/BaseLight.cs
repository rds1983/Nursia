using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Environment
{
	public abstract class BaseLight: ItemWithId
	{
		public Color Color { get; set; }

		protected BaseLight()
		{
			Color = Color.White;
		}
	}
}
