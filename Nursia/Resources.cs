using AssetManagementBase;
using FontStashSharp;
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
#if FNA
		private const string EffectsResourcePath = "EffectsSource.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "EffectsSource.MonoGameOGL.bin";
#endif

		private static AssetManager _assetManagerEffects = AssetManager.CreateResourceAssetManager(Assembly, EffectsResourcePath);
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(Assembly, "Resources");

		private static Func<Effect> _colorEffect, _skyboxEffect;
		private static Func<Effect>[] _defaultEffects = new Func<Effect>[32];
		private static Func<Effect>[] _defaultShadowMapEffects = new Func<Effect>[8];
		private static Func<Effect>[] _terrainEffects = new Func<Effect>[64];
		private static Func<Effect>[] _waterEffects = new Func<Effect>[4];
		private static Func<Effect>[] _billboardEffects = new Func<Effect>[2];
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

		public static Func<Effect> GetDefaultEffect(bool texture, bool skinning, bool clipPlane, bool lightning, bool shadows)
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

			if (shadows)
			{
				key |= 16;
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

			if (shadows)
			{
				defines["SHADOWS"] = "1";
			}

			var result = GetEffect("DefaultEffect", defines);
			_defaultEffects[key] = result;

			return result;
		}

		public static Func<Effect> GetDefaultShadowMapEffect(bool skinning, bool clipPlane)
		{
			var key = 0;

			if (clipPlane)
			{
				key |= 1;
			}

			if (skinning)
			{
				key |= 2;
			}

			if (_defaultShadowMapEffects[key] != null)
			{
				return _defaultShadowMapEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (skinning)
			{
				defines["SKINNING"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIP_PLANE"] = "1";
			}

			var result = GetEffect("DefaultEffect_ShadowMap", defines);
			_defaultShadowMapEffects[key] = result;

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

		public static Func<Effect> GetBillboardEffect(bool texture)
		{
			var key = 0;

			if (texture)
			{
				key |= 1;
			}

			if (_billboardEffects[key] != null)
			{
				return _billboardEffects[key];
			}

			var defines = new Dictionary<string, string>();
			if (texture)
			{
				defines["TEXTURE"] = "1";
			}

			var result = GetEffect("BillboardEffect", defines);
			_billboardEffects[key] = result;

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

		public static Effect GetEffect2(string name, Dictionary<string, string> defines = null)
		{
			name = Path.ChangeExtension(name, "efb");
			var result = _assetManagerEffects.LoadEffect(Nrs.GraphicsDevice, name, defines);
			return result;
		}
	}
}