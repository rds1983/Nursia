using EffectFarm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using SpriteFontPlus;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	public static class Assets
	{
		private static SpriteFont _debugFont;
		private static MultiVariantEffect _defaultMultiEffect, _waterMultiEffect;
		private static Effect _waterEffect;
		private static Effect[] _defaultEffects = new Effect[32];
		private static Texture2D _white;

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

		public static SpriteFont DebugFont
		{
			get
			{
				if (_debugFont != null)
				{
					return _debugFont;
				}

				Texture2D texture;
				using (var stream = Assembly.OpenResourceStream("Resources.Fonts.debugFont_0.png"))
				{
					texture = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
				}

				var fontData = Assembly.ReadResourceAsString("Resources.Fonts.debugFont.fnt");

				_debugFont = BMFontLoader.LoadXml(fontData, s => texture);

				return _debugFont;
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
	}
}
