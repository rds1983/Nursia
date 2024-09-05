using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using Nursia;
using Nursia.Rendering;

namespace NursiaEditor
{
	internal static class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, "Resources");
		private static Texture2D _iconDirectionalLight;
		private static SceneNode _modelAxises;

		public static Texture2D IconDirectionalLight
		{
			get
			{
				if (_iconDirectionalLight == null)
				{
					_iconDirectionalLight = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images/DirectionalLight.png");
				}

				return _iconDirectionalLight;
			}
		}

		public static SceneNode ModelAxises
		{
			get
			{
				if (_modelAxises == null)
				{
					_modelAxises = _assetManager.LoadScene("Scenes/axises.scene");
				}

				return _modelAxises;
			}
		}
	}
}
