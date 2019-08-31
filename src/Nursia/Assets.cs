using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using SpriteFontPlus;
using System.Reflection;

namespace Nursia
{
	public static class Assets
	{
		private static SpriteFont _debugFont;
		private static Effect[] _defaultEffect = new Effect[10];
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

		internal static Effect GetDefaultEffect(bool lightning, int bones)
		{
			var key = 0;
			if (lightning)
			{
				key |= 1;
			}

			if (bones > 0)
			{
				key |= bones << 1;
			}

			if (_defaultEffect[key] != null)
			{
				return _defaultEffect[key];
			}

			var resourceKey = "Resources.Effects.DefaultEffect";
			if (lightning)
			{
				resourceKey += "_LIGHTNING";
			}

			if (bones > 0)
			{
				resourceKey += "_BONES_" + bones.ToString();
			}

#if MONOGAME
			resourceKey += Nrs.IsOpenGL ? ".ogl" : ".dx11";
			resourceKey += ".mgfxo";
#else
			resourceKey += ".fxb";
#endif

			var bytes = Assembly.ReadResourceAsBytes(resourceKey);

			var result = new Effect(Nrs.GraphicsDevice, bytes);
			_defaultEffect[key] = result;
			return result;
		}
	}
}
