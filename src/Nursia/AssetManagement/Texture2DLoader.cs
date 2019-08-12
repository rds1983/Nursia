using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics2D;

namespace Nursia.AssetManagement
{
	public class Texture2DLoader : IAssetLoader<Texture2D>
	{
		public Texture2D Load(AssetLoaderContext context, string assetName)
		{
			ColorBuffer cb;

			using (var stream = context.Open(assetName))
			{
				cb = ColorBuffer.FromStream(stream);
			}

			cb.Process(true);

			return cb.CreateTexture2D();
		}
	}
}