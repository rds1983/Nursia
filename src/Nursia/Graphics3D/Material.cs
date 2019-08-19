using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public class Material : ItemWithId
	{
		public Color DiffuseColor;
		public Texture2D Texture;
		public bool IgnoreLight;

		public Material Clone()
		{
			return new Material
			{
				DiffuseColor = DiffuseColor,
				Texture = Texture
			};
		}
	}
}