using Assimp;

namespace Nursia.ModelImporter.Content
{
	class TextureContent
	{
		public string FilePath { get; set; }

		public int UVIndex { get; set; }

		public TextureMapping Mapping;
		public TextureOperation Operation;
		public TextureWrapMode WrapModeU;
		public TextureWrapMode WrapModeV;

		public TextureContent(string filePath)
		{
			FilePath = filePath;
		}
	}
}
