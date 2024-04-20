using AssetManagementBase;
using Nursia.Modelling;

namespace Nursia
{
	public static class NursiaAssetsExt
	{
		private readonly static AssetLoader<ModelInstance> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			return loader.Load(manager, assetName);
		};

		public static ModelInstance LoadGltf(this AssetManager assetManager, string path) => assetManager.UseLoader(_gltfLoader, path);
	}
}
