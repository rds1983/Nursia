using AssetManagementBase;
using Nursia.Graphics3D.Modelling;

namespace Nursia
{
	public static class NursiaAssetsExt
	{
		private readonly static AssetLoader<NursiaModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			return loader.Load(manager, assetName);
		};

		public static NursiaModel LoadGltf(this AssetManager assetManager, string path) => assetManager.UseLoader(_gltfLoader, path);
	}
}
