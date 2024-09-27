using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;

namespace Nursia.Rendering
{
	public class EffectBinding
	{
		private static int _lastBatchId = 0;
		private Effect _effect;

		public int BatchId { get; }

		public Effect Effect
		{
			get => _effect;

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				if (value == _effect)
				{
					return;
				}

				_effect = value;
				BindParameters();
			}
		}

		public EffectParameter LightViewProj { get; private set; }
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
		public EffectParameter ShadowMap { get; private set; }
		public EffectParameter Bones { get; private set; }

		public EffectBinding()
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
		}

		protected virtual void BindParameters()
		{
			LightViewProj = Effect.FindParameterByName("_lightViewProj");
			WorldViewProj = Effect.FindParameterByName("_worldViewProj");
			World = Effect.FindParameterByName("_world");
			View = Effect.FindParameterByName("_view");
			WorldInverseTranspose = Effect.FindParameterByName("_worldInverseTranspose");
			CameraPosition = Effect.FindParameterByName("_cameraPosition");

			LightType = Effect.FindParameterByName("_lightType");
			LightPosition = Effect.FindParameterByName("_lightPosition");
			LightDirection = Effect.FindParameterByName("_lightDirection");
			LightColor = Effect.FindParameterByName("_lightColor");
			LightCount = Effect.FindParameterByName("_lightCount");
			ShadowMap = Effect.FindParameterByName("_shadowMap");
			Bones = Effect.FindParameterByName("_bones");
		}
	}
}
