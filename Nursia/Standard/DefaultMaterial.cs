using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Serialization;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Standard
{
	public class DefaultMaterial : ItemWithId, IMaterial, IHasExternalAssets
	{
		private EffectBinding _effectBinding;
		private bool _receivesLight = true;
		private bool _receivesShadows = true;
		private bool _skinning = false;
		private Texture2D _texture;

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
				InvalidateDefault();
			}
		}

		[Browsable(false)]
		public string TexturePath { get; set; }


		[DefaultValue(true)]
		public bool ReceivesLight
		{
			get => _receivesLight;

			set
			{
				if (value == _receivesLight)
				{
					return;
				}

				_receivesLight = value;
				InvalidateDefault();
			}
		}

		[DefaultValue(true)]
		public bool CastsShadows { get; set; } = true;

		[DefaultValue(true)]
		public bool ReceivesShadows
		{
			get => _receivesShadows;

			set
			{
				if (value == _receivesShadows)
				{
					return;
				}

				_receivesShadows = value;
				InvalidateDefault();
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
				InvalidateDefault();
			}
		}

		public float SpecularFactor { get; set; } = 0.0f;

		public float SpecularPower { get; set; } = 250.0f;

		public Color DiffuseColor { get; set; } = Color.White;

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding EffectBinding
		{
			get
			{
				if (_effectBinding == null)
				{
					_effectBinding = Resources.GetDefaultEffectBinding(Texture != null, _skinning, false, _receivesLight, _receivesShadows)();

					var effect = _effectBinding.Effect;
					SpecularFactorParameter = effect.FindParameterByName("_specularFactor");
					SpecularPowerParameter = effect.FindParameterByName("_specularPower");
					DiffuseColorParameter = effect.FindParameterByName("_diffuseColor");
					TextureParameter = effect.FindParameterByName("_texture");
				}

				return _effectBinding;
			}
		}

		private EffectParameter SpecularFactorParameter { get; set; }
		private EffectParameter SpecularPowerParameter { get; set; }
		private EffectParameter DiffuseColorParameter { get; set; }
		private EffectParameter TextureParameter { get; set; }

		public NodeBlendMode BlendMode { get; set; } = NodeBlendMode.Opaque;

		private void InvalidateDefault()
		{
			_effectBinding = null;
		}

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(TexturePath))
			{
				Texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, TexturePath);
			}
		}

		public void SetParameters(Mesh mesh)
		{
			SpecularFactorParameter?.SetValue(SpecularFactor);
			SpecularPowerParameter?.SetValue(SpecularPower);

			DiffuseColorParameter.SetValue(DiffuseColor.ToVector4());
			TextureParameter?.SetValue(Texture);
		}
	}
}