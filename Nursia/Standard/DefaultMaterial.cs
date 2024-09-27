using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Serialization;
using Nursia.Utilities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Standard
{
	public class DefaultEffectBinding : EffectBinding
	{
		public EffectParameter SpecularFactorParameter { get; private set; }
		public EffectParameter SpecularPowerParameter { get; private set; }
		public EffectParameter DiffuseColorParameter { get; private set; }
		public EffectParameter TextureParameter { get; private set; }

		protected override void BindParameters()
		{
			base.BindParameters();

			SpecularFactorParameter = Effect.FindParameterByName("_specularFactor");
			SpecularPowerParameter = Effect.FindParameterByName("_specularPower");
			DiffuseColorParameter = Effect.FindParameterByName("_diffuseColor");
			TextureParameter = Effect.FindParameterByName("_texture");
		}
	}

	public class DefaultMaterial : BaseMaterial<DefaultEffectBinding>, IMaterial, IHasExternalAssets
	{
		private bool _receivesLight = true;
		private bool _receivesShadows = true;
		private bool _skinning = false;
		private Texture2D _texture;

		public string Id { get; set; }

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
				Invalidate();
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

		public float SpecularFactor { get; set; } = 0.0f;

		public float SpecularPower { get; set; } = 250.0f;

		public Color DiffuseColor { get; set; } = Color.White;

		public NodeBlendMode BlendMode { get; set; } = NodeBlendMode.Opaque;

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(TexturePath))
			{
				Texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, TexturePath);
			}
		}

		public void SetParameters()
		{
			var binding = InternalBinding;
			binding.SpecularFactorParameter?.SetValue(SpecularFactor);
			binding.SpecularPowerParameter?.SetValue(SpecularPower);

			binding.DiffuseColorParameter.SetValue(DiffuseColor.ToVector4());
			binding.TextureParameter?.SetValue(Texture);
		}

		protected override DefaultEffectBinding CreateBinding()
		{
			var defines = new Dictionary<string, string>();
			if (ReceivesLight)
			{
				defines["LIGHTNING"] = "1";
			}

			if (Skinning)
			{
				defines["SKINNING"] = "1";
			}

			if (Texture != null)
			{
				defines["TEXTURE"] = "1";
			}

			if (ReceivesShadows)
			{
				defines["SHADOWS"] = "1";
			}

			return EffectsRegistry.GetStockEffectBinding<DefaultEffectBinding>("DefaultEffect", defines);
		}
	}
}