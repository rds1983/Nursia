using System.IO;
using AssetManagementBase;
using Newtonsoft.Json.Linq;
using Nursia.Graphics3D;
using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Modelling;
using Nursia.Utilities;

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
