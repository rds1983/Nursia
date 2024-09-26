using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;

namespace Nursia.Rendering
{
	public class EffectBinding
	{
		private static int _lastBatchId = 0;

		public int BatchId { get; }
		public Effect Effect { get; }

		public EffectParameter LightViewProj { get; }
		public EffectParameter WorldViewProj { get; }
		public EffectParameter View { get; }
		public EffectParameter World { get; }
		public EffectParameter WorldInverseTranspose { get; }
		public EffectParameter CameraPosition { get; }
		public EffectParameter LightType { get; }
		public EffectParameter LightPosition { get; }
		public EffectParameter LightDirection { get; }
		public EffectParameter LightColor { get; }
		public EffectParameter LightCount { get; }
		public EffectParameter ShadowMap { get; }
		public EffectParameter Bones { get; }

		public EffectBinding(Effect effect)
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
			Effect = effect ?? throw new ArgumentNullException(nameof(effect));
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
