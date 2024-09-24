using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Standard
{
	public class DefaultMaterial : ItemWithId, IMaterial, ISkinnedMaterial
	{
		private class DefaultBinding : EffectBinding
		{
			public EffectParameter SpecularFactorParameter { get; set; }
			public EffectParameter SpecularPowerParameter { get; set; }
			private EffectParameter DiffuseColorParameter { get; }
			private EffectParameter TextureParameter { get; }
			private EffectParameter BonesTransformsParameter { get; }

			public DefaultBinding(Effect effect) : base(effect)
			{
				SpecularFactorParameter = effect.FindParameterByName("_specularFactor");
				SpecularPowerParameter = effect.FindParameterByName("_specularPower");
				DiffuseColorParameter = effect.FindParameterByName("_diffuseColor");
				TextureParameter = effect.FindParameterByName("_texture");
				BonesTransformsParameter = effect.FindParameterByName("_bones");
			}

			protected internal override void SetMaterialParams(IMaterial material)
			{
				base.SetMaterialParams(material);

				var defaultMaterial = (DefaultMaterial)material;

				SpecularFactorParameter?.SetValue(defaultMaterial.SpecularFactor);
				SpecularPowerParameter?.SetValue(defaultMaterial.SpecularPower);

				DiffuseColorParameter.SetValue(defaultMaterial.DiffuseColor.ToVector4());
				TextureParameter?.SetValue(defaultMaterial.Texture);

				if (BonesTransformsParameter != null && defaultMaterial.BonesTransforms != null)
				{
					BonesTransformsParameter.SetValue(defaultMaterial.BonesTransforms);
				}
			}
		}

		private class ShadowMapBinding : EffectBinding
		{
			private EffectParameter BonesTransformsParameter { get; }

			public ShadowMapBinding(Effect effect) : base(effect)
			{
				BonesTransformsParameter = effect.FindParameterByName("_bones");
			}

			protected internal override void SetMaterialParams(IMaterial material)
			{
				base.SetMaterialParams(material);

				var defaultMaterial = (DefaultMaterial)material;

				if (BonesTransformsParameter != null && defaultMaterial.BonesTransforms != null)
				{
					BonesTransformsParameter.SetValue(defaultMaterial.BonesTransforms);
				}
			}
		}

		private EffectBinding _defaultBinding, _shadowMapBinding;
		private bool _receivesLight = true;
		private bool _skinning = false;
		private bool _receivesShadows = true;
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
				InvalidateShadowMap();
			}
		}

		public float SpecularFactor { get; set; } = 0.0f;

		public float SpecularPower { get; set; } = 250.0f;

		public Color DiffuseColor { get; set; } = Color.White;

		[Browsable(false)]
		[JsonIgnore]
		public Matrix[] BonesTransforms { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding DefaultEffect
		{
			get
			{
				if (_defaultBinding == null)
				{
					var effect = Resources.GetDefaultEffect(Texture != null, _skinning, false, _receivesLight, _receivesShadows)();
					_defaultBinding = new DefaultBinding(effect);
				}

				return _defaultBinding;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding ShadowMapEffect
		{
			get
			{
				if (_shadowMapBinding == null)
				{
					var effect = Resources.GetDefaultShadowMapEffect(_skinning, false)();
					_shadowMapBinding = new ShadowMapBinding(effect);
				}

				return _shadowMapBinding;
			}
		}

		private void InvalidateDefault()
		{
			_defaultBinding = null;
		}

		private void InvalidateShadowMap()
		{
			_shadowMapBinding = null;
		}

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(TexturePath))
			{
				Texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, TexturePath);
			}
		}
	}
}