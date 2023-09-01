using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D
{
	public class Material : ItemWithId
	{
		public Color DiffuseColor;
		public Texture2D Texture;

		public Material(Color diffuseColor, Texture2D texture = null)
		{
			DiffuseColor = diffuseColor;
			Texture = texture;
		}

		public Material Clone()
		{
			return new Material(DiffuseColor, Texture);
		}
	}
}