using Nursia.Graphics2D;

namespace Nursia.AssetManagement
{
	public class ColorBufferLoader: IAssetLoader<ColorBuffer>
	{
		public ColorBuffer Load(AssetLoaderContext context, string assetName)
		{
			ColorBuffer image;
			using (var stream = context.Open(assetName))
			{
				image = ColorBuffer.FromStream(stream);
			}

			return image;
		}
	}
}
