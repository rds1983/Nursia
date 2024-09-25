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
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		private static AssetManager _assetManagerEffectBindings = AssetManager.CreateResourceAssetManager(Assembly, EffectsResourcePath);
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(Assembly, "Resources");

		private static Func<EffectBinding> _colorEffectBinding, _skyboxEffectBinding;
		private static Func<EffectBinding>[] _defaultEffectBindings = new Func<EffectBinding>[32];
		private static Func<EffectBinding>[] _defaultShadowMapEffectBindings = new Func<EffectBinding>[8];
		private static Func<EffectBinding>[] _terrainEffectBindings = new Func<EffectBinding>[64];
		private static Func<EffectBinding>[] _waterEffectBindings = new Func<EffectBinding>[4];
		private static Func<EffectBinding>[] _billboardEffectBindings = new Func<EffectBinding>[2];
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

		public static Func<EffectBinding> ColorEffectBinding
		{
			get
			{
				if (_colorEffectBinding != null)
				{
					return _colorEffectBinding;
				}

				_colorEffectBinding = GetEffectBinding("ColorEffect");
				return _colorEffectBinding;
			}
		}

		public static Func<EffectBinding> SkyboxEffectBinding
		{
			get
			{
				if (_skyboxEffectBinding != null)
				{
					return _skyboxEffectBinding;
				}

				_skyboxEffectBinding = GetEffectBinding("SkyboxEffect");
				return _skyboxEffectBinding;
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

		public static Func<EffectBinding> GetDefaultEffectBinding(bool texture, bool skinning, bool clipPlane, bool lightning, bool shadows)
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

			if (_defaultEffectBindings[key] != null)
			{
				return _defaultEffectBindings[key];
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

			var result = GetEffectBinding("DefaultEffect", defines);
			_defaultEffectBindings[key] = result;

			return result;
		}

		public static Func<EffectBinding> GetShadowMapEffectBinding(bool skinning, bool clipPlane)
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

			if (_defaultShadowMapEffectBindings[key] != null)
			{
				return _defaultShadowMapEffectBindings[key];
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

			var result = GetEffectBinding("ShadowMap", defines);
			_defaultShadowMapEffectBindings[key] = result;

			return result;
		}

		public static Func<EffectBinding> GetTerrainEffectBinding(int texturesCount, bool clipPlane, bool marker, bool lightning)
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


			if (_terrainEffectBindings[key] != null)
			{
				return _terrainEffectBindings[key];
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

			var result = GetEffectBinding("TerrainEffect", defines);
			_terrainEffectBindings[key] = result;

			return result;
		}

		public static Func<EffectBinding> GetWaterEffectBinding(bool depthBuffer, bool cubeMapReflection)
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

			if (_waterEffectBindings[key] != null)
			{
				return _waterEffectBindings[key];
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

			var result = GetEffectBinding("WaterEffect", defines);
			_waterEffectBindings[key] = result;

			return result;
		}

		public static Func<EffectBinding> GetBillboardEffectBinding(bool texture)
		{
			var key = 0;

			if (texture)
			{
				key |= 1;
			}

			if (_billboardEffectBindings[key] != null)
			{
				return _billboardEffectBindings[key];
			}

			var defines = new Dictionary<string, string>();
			if (texture)
			{
				defines["TEXTURE"] = "1";
			}

			var result = GetEffectBinding("BillboardEffect", defines);
			_billboardEffectBindings[key] = result;

			return result;
		}

		private static Func<EffectBinding> GetEffectBinding(string name, Dictionary<string, string> defines = null)
		{
			name = Path.ChangeExtension(name, "efb");
			var result = _assetManagerEffectBindings.LoadEffectBinding(name, defines);
			return () => result;
		}
	}
}