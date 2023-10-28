using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia
{
	internal static class Resources
	{
		private static AssetManager _assetManagerEffects = AssetManager.CreateResourceAssetManager(Assembly, "EffectsSource.FNA.bin");
		private static AssetManager _assetManagerResources = AssetManager.CreateResourceAssetManager(Assembly, "Resources");
		private static Func<Effect> _colorEffect, _skyboxEffect;
		private static Func<Effect>[] _defaultEffects = new Func<Effect>[16];
		private static Func<Effect>[] _terrainEffects = new Func<Effect>[64];
		private static Func<Effect>[] _waterEffects = new Func<Effect>[4];
		private static Texture2D _white, _waterDudv, _waterNormals;

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

		public static Func<Effect> ColorEffect
		{
			get
			{
				if (_colorEffect != null)
				{
					return _colorEffect;
				}

				_colorEffect = GetEffect("ColorEffect");
				return _colorEffect;
			}
		}

		public static Func<Effect> SkyboxEffect
		{
			get
			{
				if (_skyboxEffect != null)
				{
					return _skyboxEffect;
				}

				_skyboxEffect = GetEffect("SkyboxEffect");
				return _skyboxEffect;
			}
		}

		public static Texture2D WaterDudv
		{
			get
			{
				if (_waterDudv == null)
				{
					_waterDudv = _assetManagerResources.LoadTexture2D(Nrs.GraphicsDevice, "Images.waterDUDV.png");
				}

				return _waterDudv;
			}
		}

		public static Texture2D WaterNormals
		{
			get
			{
				if (_waterNormals == null)
				{
					_waterNormals = _assetManagerResources.LoadTexture2D(Nrs.GraphicsDevice, "Images.waterNormals.png");
				}

				return _waterNormals;
			}
		}

		public static Func<Effect> GetDefaultEffect(bool texture, bool skinning, bool clipPlane, bool lightning)
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

			var result = GetEffect("DefaultEffect", defines);
			_defaultEffects[key] = result;

			return result;
		}

		public static Func<Effect> GetTerrainEffect(int texturesCount, bool clipPlane, bool marker, bool lightning)
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

			var result = GetEffect("TerrainEffect", defines);
			_terrainEffects[key] = result;

			return result;
		}

		public static Func<Effect> GetWaterEffect(bool depthBuffer, bool cubeMapReflection)
		{
			var key = 0;

			if (depthBuffer)
			{
				key |= 1;
			}

			if (cubeMapReflection)
			{
				key |= 2;
			}

			if (_waterEffects[key] != null)
			{
				return _waterEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (depthBuffer)
			{
				defines["DEPTH_BUFFER"] = "1";
			}

			if (cubeMapReflection)
			{
				defines["CUBEMAP_REFLECTION"] = "1";
			}
			else
			{
				defines["PLANAR_REFLECTION"] = "1";
			}

			var result = GetEffect("WaterEffect", defines);
			_waterEffects[key] = result;

			return result;
		}

		private static Func<Effect> GetEffect(string name, Dictionary<string, string> defines = null)
		{
			if (Nrs.ExternalEffectsSource == null)
			{
				name = Path.ChangeExtension(name, "efb");
				var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, name, defines);
				return () => result;
			}

			return Nrs.ExternalEffectsSource.Get(Nrs.GraphicsDevice, name, defines);
		}
	}
}