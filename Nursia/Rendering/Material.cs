using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Utilities;
using System;
using System.ComponentModel;

namespace Nursia.Rendering
{
	public class MaterialCommonParameters
	{
		private Effect _effect;

		public EffectParameter WorldViewProj { get; private set; }
		public EffectParameter View { get; private set; }
		public EffectParameter World { get; private set; }
		public EffectParameter WorldInverseTranspose { get; private set; }
		public EffectParameter CameraPosition { get; private set; }
		public EffectParameter LightType { get; private set; }
		public EffectParameter LightPosition { get; private set; }
		public EffectParameter LightDirection { get; private set; }
		public EffectParameter LightColor { get; private set; }
		public EffectParameter LightCount { get; private set; }

		internal Effect Effect
		{
			get => _effect;

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (value == _effect)
				{
					return;
				}

				_effect = value;
				WorldViewProj = _effect.FindParameterByName("_worldViewProj");
				World = _effect.FindParameterByName("_world");
				View = _effect.FindParameterByName("_view");
				WorldInverseTranspose = _effect.FindParameterByName("_worldInverseTranspose");
				CameraPosition = _effect.FindParameterByName("_cameraPosition");

				LightType = _effect.FindParameterByName("_lightType");
				LightPosition = _effect.FindParameterByName("_lightPosition");
				LightDirection = _effect.FindParameterByName("_lightDirection");
				LightColor = _effect.FindParameterByName("_lightColor");
				LightCount = _effect.FindParameterByName("_lightCount");
			}
		}
	}

	public abstract class Material : ItemWithId
	{
		private bool _dirty = true;
		private readonly MaterialCommonParameters _commonParameters = new MaterialCommonParameters();

		[Browsable(false)]
		[JsonIgnore]
		public MaterialCommonParameters CommonParameters
		{
			get
			{
				UpdateEffect();
				return _commonParameters;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public Effect Effect => CommonParameters.Effect;

		[Category("Behavior")]
		public float SpecularFactor { get; set; } = 0.0f;

		[Category("Behavior")]
		public float SpecularPower { get; set; } = 250.0f;

		[Browsable(false)]
		[JsonIgnore]
		public EffectTechnique DefaultTechnique { get; private set; }

		[Browsable(false)]
		[JsonIgnore]
		public EffectTechnique ShadowMapTechnique { get; private set; }

		private EffectParameter SpecularFactorParameter { get; set; }
		private EffectParameter SpecularPowerParameter { get; set; }


		protected void Invalidate()
		{
			_dirty = true;
		}

		protected abstract Effect CreateEffect();

		protected internal virtual void SetMaterialParameters()
		{
			SpecularFactorParameter?.SetValue(SpecularFactor);
			SpecularPowerParameter?.SetValue(SpecularPower);
		}

		private void UpdateEffect()
		{
			if (!_dirty)
			{
				return;
			}

			var effect = CreateEffect();

			_commonParameters.Effect = effect;
			SpecularFactorParameter = effect.FindParameterByName("_specularFactor");
			SpecularPowerParameter = effect.FindParameterByName("_specularPower");

			DefaultTechnique = effect.Techniques["Default"];
			ShadowMapTechnique = effect.Techniques["ShadowMap"];

			_dirty = false;
		}
	}
}