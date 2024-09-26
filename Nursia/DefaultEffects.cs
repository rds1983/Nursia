using Nursia.Rendering;
using System;
using System.Collections.Generic;

namespace Nursia
{
	internal static class DefaultEffects
	{
		private static EffectBinding _colorEffectBinding, _skyboxEffectBinding;
		private static EffectBinding[] _defaultEffectBindings = new EffectBinding[32];
		private static EffectBinding[] _defaultShadowMapEffectBindings = new EffectBinding[8];
		private static EffectBinding[] _terrainEffectBindings = new EffectBinding[64];
		private static EffectBinding[] _waterEffectBindings = new EffectBinding[4];
		private static EffectBinding[] _billboardEffectBindings = new EffectBinding[2];


		public static EffectBinding ColorEffectBinding
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

		public static EffectBinding SkyboxEffectBinding
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

		public static EffectBinding GetDefaultEffectBinding(bool texture, bool skinning, bool clipPlane, bool lightning, bool shadows)
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

		public static EffectBinding GetShadowMapEffectBinding(bool skinning, bool clipPlane)
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

		public static EffectBinding GetTerrainEffectBinding(int texturesCount, bool clipPlane, bool marker, bool lightning)
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

		public static EffectBinding GetWaterEffectBinding(bool depthBuffer, bool cubeMapReflection)
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

		public static EffectBinding GetBillboardEffectBinding(bool texture)
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

		private static EffectBinding GetEffectBinding(string name, Dictionary<string, string> defines = null) =>
			Nrs.EffectsRegistry.GetEffectBinding(typeof(DefaultEffects).Assembly, name, defines);
	}
}