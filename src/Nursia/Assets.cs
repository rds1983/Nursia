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
		private static EffectsRepository _effectsRepository;
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

		internal static EffectsRepository EffectsRepository
		{
			get
			{
				if (_effectsRepository != null)
				{
					return _effectsRepository;
				}

				_effectsRepository = EffectsRepository.CreateFromFolder(@"D:\Projects\Nursia\src\Nursia\EffectsSource\MonoGameOGL");
				return _effectsRepository;
			}
		}

		internal static Effect WaterEffect
		{
			get
			{
				if (_waterEffect != null)
				{
					return _waterEffect;
				}

				_waterEffect = EffectsRepository.Get(Nrs.GraphicsDevice, "WaterEffect");
				return _waterEffect;
			}
		}

		internal static Effect SkyboxEffect
		{
			get
			{
				if (_skyboxEffect != null)
				{
					return _skyboxEffect;
				}

				_skyboxEffect = EffectsRepository.Get(Nrs.GraphicsDevice, "SkyboxEffect");
				return _skyboxEffect;
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

			var result = EffectsRepository.Get(Nrs.GraphicsDevice, "DefaultEffect", defines);

			_defaultEffects[key] = result;
			return result;
		}
	}
}