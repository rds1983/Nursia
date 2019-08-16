using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia
{
	internal static class Assets
	{
		private static Effect[] _defaultEffect = new Effect[4];

		public static Effect GetDefaultEffect(bool lightning, bool texture)
		{
			var key = 0;
			if (lightning)
			{
				key |= 1;
			}

			if (texture)
			{
				key |= 2;
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

			if (texture)
			{
				resourceKey += "_TEXTURE";
			}

#if MONOGAME
			resourceKey += Nrs.IsOpenGL ? ".ogl" : ".dx11";
			resourceKey += ".mgfxo";
#endif

			var bytes = typeof(Assets).Assembly.ReadAsBytes(resourceKey);

			var result = new Effect(Nrs.GraphicsDevice, bytes);
			_defaultEffect[key] = result;
			return result;
		}
	}
}
