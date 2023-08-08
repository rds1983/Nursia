namespace Nursia.Graphics3D.Modelling
{
	partial class NursiaModel
	{
		public static NursiaModel LoadFromGltf(string path)
		{
			var loader = new GltfLoader();

			return loader.Load(path);
		}
	}
}