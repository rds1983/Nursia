using AssetManagementBase;
using Nursia.Cameras;
using Nursia.Utilities;
using System.ComponentModel;
using System.IO;

namespace Nursia.Rendering
{
	/// <summary>
	/// Class describing the 3D scene
	/// </summary>
	public class Scene : SceneNode
	{
		[Browsable(false)]
		public Camera Camera { get; } = new PerspectiveCamera();

		public void SaveToFile(string path)
		{
			var options = JsonExtensions.CreateOptions();
			JsonExtensions.SerializeToFile(path, options, this);
		}

		public static Scene ReadFromString(string data, AssetManager assetManager)
		{
			var options = JsonExtensions.CreateOptions();
			var result = JsonExtensions.DeserializeFromString<Scene>(data, options);

			result.Load(assetManager);

			return result;
		}

		public static Scene ReadFromFile(string path, AssetManager assetManager)
		{
			var data = File.ReadAllText(path);
			return ReadFromString(data, assetManager);
		}
	}
}
