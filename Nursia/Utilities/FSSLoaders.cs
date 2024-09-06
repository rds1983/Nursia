using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Nursia
{
	internal static class FSSLoaders
	{
		private class FontSystemLoadingSettings : IAssetSettings
		{
			public Texture2D ExistingTexture { get; set; }
			public Rectangle ExistingTextureUsedSpace { get; set; }
			public string[] AdditionalFonts { get; set; }

			public string BuildKey() => string.Empty;
		}

		private static AssetLoader<FontSystem> _fontSystemLoader = (manager, assetName, settings, tag) =>
		{
			var fontSystemSettings = new FontSystemSettings();

			var fontSystemLoadingSettings = (FontSystemLoadingSettings)settings;
			if (fontSystemLoadingSettings != null)
			{
				fontSystemSettings.ExistingTexture = fontSystemLoadingSettings.ExistingTexture;
				fontSystemSettings.ExistingTextureUsedSpace = fontSystemLoadingSettings.ExistingTextureUsedSpace;
			};

			var fontSystem = new FontSystem(fontSystemSettings);
			var data = manager.ReadAsByteArray(assetName);
			fontSystem.AddFont(data);
			if (fontSystemLoadingSettings != null && fontSystemLoadingSettings.AdditionalFonts != null)
			{
				foreach (var file in fontSystemLoadingSettings.AdditionalFonts)
				{
					data = manager.LoadByteArray(file, false);
					fontSystem.AddFont(data);
				}
			}

			return fontSystem;
		};

		private static AssetLoader<StaticSpriteFont> _staticFontLoader = (manager, assetName, settings, tag) =>
		{
			var fontData = manager.ReadAsString(assetName);
			var graphicsDevice = (GraphicsDevice)tag;

			return StaticSpriteFont.FromBMFont(fontData,
						name =>
						{
							var imageData = manager.LoadByteArray(name, false);
							return new MemoryStream(imageData);
						},
						graphicsDevice);
		};

		public static FontSystem LoadFontSystem(this AssetManager assetManager, string assetName, string[] additionalFonts = null, Texture2D existingTexture = null, Rectangle existingTextureUsedSpace = default(Rectangle))
		{
			FontSystemLoadingSettings settings = null;
			if (additionalFonts != null || existingTexture != null)
			{
				settings = new FontSystemLoadingSettings
				{
					AdditionalFonts = additionalFonts,
					ExistingTexture = existingTexture,
					ExistingTextureUsedSpace = existingTextureUsedSpace
				};
			}

			return assetManager.UseLoader(_fontSystemLoader, assetName, settings);
		}

		public static StaticSpriteFont LoadStaticSpriteFont(this AssetManager assetManager, GraphicsDevice graphicsDevice, string assetName)
		{
			return assetManager.UseLoader(_staticFontLoader, assetName, tag: graphicsDevice);
		}
	}
}
