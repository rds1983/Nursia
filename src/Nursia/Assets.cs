using EffectFarm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	public static class Assets
	{
		private static MultiVariantEffect 
			_defaultMultiEffect, 
			_waterMultiEffect,
			_skyboxMultiEffect;
		private static Effect _waterEffect, _skyboxEffect;
		private static Effect[] _defaultEffects = new Effect[32];
		private static Texture2D _white, _waterDUDV, _waterNormals;

		private static Assembly Assembly
		{
			get
			{
				return typeof(Assets).Assembly;
			}
		}

		internal static Texture2D White
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

		internal static Texture2D WaterDUDV
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

		internal static Texture2D WaterNormals
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

		internal static Effect GetDefaultEffect(bool clipPlane, bool lightning, int bones)
		{
			var key = 0;

			if (clipPlane)
			{
				key |= 1;
			}

			if (lightning)
			{
				key |= 2;
			}

			if (bones > 0)
			{
				key |= bones << 2;
			}

			if (_defaultEffects[key] != null)
			{
				return _defaultEffects[key];
			}

			if (_defaultMultiEffect == null)
			{
				_defaultMultiEffect = new MultiVariantEffect(() =>
				{
					return Assembly.OpenResourceStream("Resources.Effects.DefaultEffect.efb");
				});
			}

			var defines = new Dictionary<string, string>();

			if (clipPlane)
			{
				defines["CLIP_PLANE"] = "1";
			}

			if (lightning)
			{
				defines["LIGHTNING"] = "1";
			}

			if (bones > 0)
			{
				defines["BONES"] = bones.ToString();
			}

			var result = _defaultMultiEffect.GetEffect(Nrs.GraphicsDevice, defines);

			_defaultEffects[key] = result;
			return result;
		}

		internal static Effect GetWaterEffect()
		{
			if (_waterEffect != null)
			{
				return _waterEffect;
			}

			if (_waterMultiEffect == null)
			{
				_waterMultiEffect = new MultiVariantEffect(() =>
				{
					return Assembly.OpenResourceStream("Resources.Effects.WaterEffect.efb");
				});
			}

			_waterEffect = _waterMultiEffect.GetEffect(Nrs.GraphicsDevice, null);
			return _waterEffect;
		}

		internal static Effect GetSkyboxEffect()
		{
			if (_skyboxEffect != null)
			{
				return _skyboxEffect;
			}

			if (_skyboxMultiEffect == null)
			{
				_skyboxMultiEffect = new MultiVariantEffect(() =>
				{
					return Assembly.OpenResourceStream("Resources.Effects.SkyboxEffect.efb");
				});
			}

			_skyboxEffect = _skyboxMultiEffect.GetEffect(Nrs.GraphicsDevice, null);
			return _skyboxEffect;
		}
	}
}
