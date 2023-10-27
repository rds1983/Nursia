using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	internal static class Resources
	{
		private static AssetManager _assetManagerEffects = AssetManager.CreateResourceAssetManager(Assembly, "EffectsSource.FNA.bin");
		private static Effect _colorEffect, _skyboxEffect;
		private static Effect[] _defaultEffects = new Effect[16];
		private static Effect[] _terrainEffects = new Effect[64];
		private static Effect[] _waterEffects = new Effect[4];
		private static Texture2D _white, _waterNormals1, _waterNormals2;

		private static Assembly Assembly
		{
			get
			{
				return typeof(Resources).Assembly;
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

		public static Effect ColorEffect
		{
			get
			{
				if (_colorEffect != null)
				{
					return _colorEffect;
				}

				_colorEffect = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "ColorEffect.efb");
				return _colorEffect;
			}
		}

		public static Effect SkyboxEffect
		{
			get
			{
				if (_skyboxEffect != null)
				{
					return _skyboxEffect;
				}

				_skyboxEffect = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "SkyboxEffect.efb");
				return _skyboxEffect;
			}
		}

		public static Texture2D WaterNormals1
		{
			get
			{
				if (_waterNormals1 == null)
				{
					using (var stream = Assembly.OpenResourceStream("Resources.Images.waterNormals1.png"))
					{
						_waterNormals1 = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
					}
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
					using (var stream = Assembly.OpenResourceStream("Resources.Images.waterNormals2.png"))
					{
						_waterNormals2 = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
					}
				}

				return _waterNormals2;
			}
		}

		public static Effect GetDefaultEffect(bool texture, bool skinning, bool clipPlane, bool lightning)
		{
			var key = 0;

			if (lightning)
			{
				key |= 1;
			}

			if (clipPlane)
			{
				key |= 2;
			}

			if (skinning)
			{
				key |= 4;
			}

			if (texture)
			{
				key |= 8;
			}

			if (_defaultEffects[key] != null)
			{
				return _defaultEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (lightning)
			{
				defines["LIGHTNING"] = "1";
			}

			if (skinning)
			{
				defines["SKINNING"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIP_PLANE"] = "1";
			}

			if (texture)
			{
				defines["TEXTURE"] = "1";
			}

			var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "DefaultEffect.efb", defines);
			_defaultEffects[key] = result;

			return result;
		}

		public static Effect GetTerrainEffect(int texturesCount, bool clipPlane, bool marker, bool lightning)
		{
			if (texturesCount < 0 || texturesCount > 4)
			{
				throw new ArgumentOutOfRangeException(nameof(texturesCount));
			}

			var key = 0;
			key |= texturesCount;

			if (lightning)
			{
				key |= 8;
			}

			if (clipPlane)
			{
				key |= 16;
			}

			if (marker)
			{
				key |= 32;
			}


			if (_terrainEffects[key] != null)
			{
				return _terrainEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (lightning)
			{
				defines["LIGHTNING"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIP_PLANE"] = "1";
			}

			if (marker)
			{
				defines["MARKER"] = "1";
			}

			if (texturesCount > 0)
			{
				defines["TEXTURES"] = texturesCount.ToString();
			}

			var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "TerrainEffect.efb", defines);
			_terrainEffects[key] = result;

			return result;
		}

		public static Effect GetWaterEffect(bool waves, bool depthBuffer)
		{
			var key = 0;
			if (waves)
			{
				key |= 1;
			}

			if (depthBuffer)
			{
				key |= 2;
			}

			if (_waterEffects[key] != null)
			{
				return _waterEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (waves)
			{
				defines["WAVES"] = "1";
			}

			if (depthBuffer)
			{
				defines["DEPTH_BUFFER"] = "1";
			}

			var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "WaterEffect.efb", defines);
			_waterEffects[key] = result;

			return result;
		}
	}
}