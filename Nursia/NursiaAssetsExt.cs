using AssetManagementBase;
using Nursia.Modelling;
using Nursia.Rendering;
using System.Collections.Generic;

namespace Nursia
{
	public static class NursiaAssetsExt
	{
		private readonly static AssetLoader<ModelInstance> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			return loader.Load(manager, assetName);
		};

		private readonly static AssetLoader<Scene> _sceneLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.LoadString(assetName);
			return Scene.ReadFromString(data, manager);
		};

		public static ModelInstance LoadGltf(this AssetManager assetManager,
			string path) => assetManager.UseLoader(_gltfLoader, path);

		public static Scene LoadScene(this AssetManager assetManager,
			string path) => assetManager.UseLoader(_sceneLoader, path);

		public static EffectBinding LoadEffectBinding(this AssetManager assetManager, string name, Dictionary<string, string> defines = null)
		{
			return new EffectBinding(assetManager.LoadEffect(Nrs.GraphicsDevice, name, defines));
		}
	}
}
