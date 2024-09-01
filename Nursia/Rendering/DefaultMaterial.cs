using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Rendering
{
	public class DefaultMaterial : Material, ISkinnedMaterial
	{
		private bool _lightning = true;
		private bool _skinning = false;
		private Texture2D _texture;

		[Browsable(false)]
		[JsonIgnore]
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

		[Browsable(false)]
		[JsonIgnore]
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

		public Color DiffuseColor { get; set; } = Color.White;

		[Browsable(false)]
		[JsonIgnore]
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
