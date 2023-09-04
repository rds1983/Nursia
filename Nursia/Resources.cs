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
		private static AssetManager _assetManagerEffects = AssetManager.CreateResourceAssetManager(Assembly, "EffectsSource.FNA");
		private static Effect _colorEffect, _waterEffect, _skyboxEffect;
		private static Effect[] _defaultEffects = new Effect[256];
		private static Effect[] _terrainEffects = new Effect[20];
		private static Texture2D _white, _waterDUDV, _waterNormals;

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

		public static Effect WaterEffect
		{
			get
			{
				if (_waterEffect != null)
				{
					return _waterEffect;
				}

				_waterEffect = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "WaterEffect.efb");
				return _waterEffect;
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

		public static Texture2D WaterDUDV
		{
			get
			{
				if (_waterDUDV == null)
				{
					using (var stream = Assembly.OpenResourceStream("Resources.Images.waterDUDV.png"))
					{
						_waterDUDV = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
					}
				}

				return _waterDUDV;
			}
		}

		public static Texture2D WaterNormals
		{
			get
			{
				if (_waterNormals == null)
				{
					using (var stream = Assembly.OpenResourceStream("Resources.Images.waterNormals.png"))
					{
						_waterNormals = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
					}
				}

				return _waterNormals;
			}
		}

		public static Effect GetDefaultEffect(bool texture, bool skinning, bool clipPlane, bool directLight, int pointLights)
		{
			var key = 0;

			key |= pointLights;

			if (directLight)
			{
				key |= 16;
			}

			if (clipPlane)
			{
				key |= 32;
			}

			if (skinning)
			{
				key |= 64;
			}

			if (texture)
			{
				key |= 128;
			}

			if (_defaultEffects[key] != null)
			{
				return _defaultEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (pointLights > 0)
			{
				defines["POINT_LIGHTS"] = pointLights.ToString();
			}

			if (directLight)
			{
				defines["DIR_LIGHT"] = "1";
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

		public static Effect GetTerrainEffect(int texturesCount, bool clipPlane, bool directLight)
		{
			if (texturesCount < 0 || texturesCount > 4)
			{
				throw new ArgumentOutOfRangeException(nameof(texturesCount));
			}

			var key = 0;
			if (directLight)
			{
				key |= 1;
			}

			if (clipPlane)
			{
				key |= 2;
			}

			key |= (texturesCount << 2);

			if (_terrainEffects[key] != null)
			{
				return _terrainEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (directLight)
			{
				defines["DIRECT_LIGHT"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIP_PLANE"] = "1";
			}

			var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, "TerrainEffect.efb", defines);
			_terrainEffects[key] = result;

			return result;
		}
	}
}