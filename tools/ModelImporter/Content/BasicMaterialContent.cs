using Microsoft.Xna.Framework;

namespace Nursia.ModelImporter.Content
{
	class MaterialContent : BaseContent
	{
		public TextureContent Texture { get; set; }
		public TextureContent TransparencyTexture { get; set; }
		public TextureContent SpecularTexture { get; set; }
		public TextureContent BumpTexture { get; set; }

		public Vector3? DiffuseColor { get; set; }
		public Vector3? EmissiveColor { get; set; }
		public Vector3? SpecularColor { get; set; }

		public float Alpha { get; set; }
		public float SpecularPower { get; set; }

		public MaterialContent()
		{
			Alpha = 1.0f;
		}
	}
}