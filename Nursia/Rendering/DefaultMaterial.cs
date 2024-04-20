using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia.Rendering
{
	public class DefaultMaterial : Material, ISkinnedMaterial
	{
		private bool _lightning = true;
		private bool _skinning = false;
		private Texture2D _texture;

		public Texture2D Texture
		{
			get => _texture;

			set
			{
				if (value == _texture)
				{
					return;
				}

				_texture = value;
				Invalidate();
			}
		}

		public bool Lightning
		{
			get => _lightning;

			set
			{
				if (value == _lightning)
				{
					return;
				}

				_lightning = value;
				Invalidate();
			}
		}

		public bool Skinning
		{
			get => _skinning;

			set
			{
				if (value == _skinning)
				{
					return;
				}

				_skinning = value;
				Invalidate();
			}
		}

		public Color DiffuseColor { get; set; }
		public Matrix[] BonesTransforms { get; set; }

		private EffectParameter DiffuseColorParameter { get; set; }
		private EffectParameter TextureParameter { get; set; }
		private EffectParameter BonesTransformsParameter { get; set; }

		protected override Effect CreateEffect()
		{
			var result = Resources.GetDefaultEffect(Texture != null, _skinning, false, _lightning)();

			DiffuseColorParameter = result.FindParameterByName("_diffuseColor");
			TextureParameter = result.FindParameterByName("_texture");
			BonesTransformsParameter = result.FindParameterByName("_bones");

			return result;
		}

		protected internal override void SetMaterialParameters()
		{
			base.SetMaterialParameters();

			DiffuseColorParameter.SetValue(DiffuseColor.ToVector4());

			TextureParameter?.SetValue(Texture);

			if (BonesTransformsParameter != null && BonesTransforms != null)
			{
				BonesTransformsParameter.SetValue(BonesTransforms);
			}
		}
	}
}
