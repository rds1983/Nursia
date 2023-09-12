using System.Collections.Generic;
using System.IO;
using AssetManagementBase;
using Nursia.Graphics3D.Modelling;

namespace Nursia
{
	public static class ModelStorage
	{
		public static Dictionary<string, NursiaModel> Models { get; } = new Dictionary<string, NursiaModel>();

		public static void Load(string path)
		{
			Models.Clear();

			var assetManager = AssetManager.CreateFileAssetManager(path);

			var files = Directory.EnumerateFiles(path);
			foreach (var file in files)
			{
				if (!file.EndsWith(".glb") && !file.EndsWith(".gltf"))
				{
					continue;
				}

				var name = Path.GetFileName(file);
				var model = assetManager.LoadGltf(name);
				Models[Path.GetFileNameWithoutExtension(name)] = model;
			}
		}
	}
}
