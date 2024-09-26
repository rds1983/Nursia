using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia
{

	internal static class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(Assembly, "Resources");

		private static Texture2D _white, _waterNormals1, _waterNormals2;
		private static FontSystem _fontSystem;
		private static SpriteBatch _spriteBatch;

		private static Assembly Assembly
		{
			get
			{
				return typeof(Resources).Assembly;
			}
		}

		public static FontSystem DefaultFontSystem
		{
			get
			{
				if (_fontSystem == null)
				{
					_fontSystem = _assetManager.LoadFontSystem("Fonts/Inter-Regular.ttf");
				}

				return _fontSystem;
			}
		}

		public static Texture2D White
		{
			get
			{
				if (_white == null)
				{
					_white = new Texture2D(Nrs.GraphicsDevice, 1, 1);
					_white.SetData(new[] { Color.White });
				}

				return _white;
			}
		}

		public static Texture2D WaterNormals1
		{
			get
			{
				if (_waterNormals1 == null)
				{
					_waterNormals1 = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.waterNormals1.png");
				}

				return _waterNormals1;
			}
		}

		public static Texture2D WaterNormals2
		{
			get
			{
				if (_waterNormals2 == null)
				{
					using (var stream = Assembly.OpenResourceStream("Images.waterNormals2.png"))
					{
						_waterNormals2 = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
					}
				}

				return _waterNormals2;
			}
		}

		public static SpriteBatch SpriteBatch
		{
			get
			{
				if (_spriteBatch == null)
				{
					_spriteBatch = new SpriteBatch(Nrs.GraphicsDevice);
				}

				return _spriteBatch;
			}
		}
	}
}