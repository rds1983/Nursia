using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using Nursia;

namespace NursiaEditor
{
	internal static class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, "Resources");
		private static Texture2D _iconDirectionalLight;

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
	}
}
