using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;

namespace Nursia.Graphics3D
{
	public class Material : ItemWithId
	{
		public Color DiffuseColor;
		public Texture2D Texture;

		[Category("Behavior")]
		public float SpecularFactor { get; set; } = 0.0f;

		[Category("Behavior")]
		public float SpecularPower { get; set; } = 250.0f;

		public Material(Color diffuseColor, Texture2D texture = null)
		{
			DiffuseColor = diffuseColor;
			Texture = texture;
		}

		public Material Clone()
		{
			return new Material(DiffuseColor, Texture);
		}

		public static Material CreateSolidMaterial(Color color) => new Material(color);
	}
}