using System.IO;

namespace Nursia.AssetManagement
{
	public interface IAssetResolver
	{
		Stream Open(string assetName);
	}
}
