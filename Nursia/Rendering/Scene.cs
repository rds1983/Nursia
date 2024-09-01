using AssetManagementBase;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Rendering
{
	/// <summary>
	/// Class describing the 3D scene
	/// </summary>
	public class Scene : SceneNode
	{
		[Browsable(false)]
		public Camera Camera { get; } = new Camera();

		public void SaveToFile(string path)
		{
			var options = JsonExtensions.CreateOptions();
			JsonExtensions.SerializeToFile(path, options, this);
		}

		public static Scene ReadFromFile(string path, AssetManager assetManager)
		{
			var options = JsonExtensions.CreateOptions();
			var result = JsonExtensions.DeserializeFromFile<Scene>(path, options);

			result.Load(assetManager);

			return result;
		}
	}
}
