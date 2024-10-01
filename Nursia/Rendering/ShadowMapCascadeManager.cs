using Microsoft.Xna.Framework;
using Nursia.Rendering.Lights;
using System;

namespace Nursia.Rendering
{
	internal class ShadowMapCascadeManager
	{
		private readonly float[] _distances = new float[Constants.ShadowMapCascadesCount];
		private readonly Matrix[] _sourceViewProjs = new Matrix[Constants.ShadowMapCascadesCount];
		private readonly Matrix[] _lightViews = new Matrix[Constants.ShadowMapCascadesCount];
		private readonly Matrix[] _lightProjs = new Matrix[Constants.ShadowMapCascadesCount];
		private readonly Matrix[] _lightViewProjs = new Matrix[Constants.ShadowMapCascadesCount];

		public float[] Distances => _distances;
		public Matrix[] SourceViewProjs => _sourceViewProjs;
		public Matrix[] LightViews => _lightViews;
		public Matrix[] LightProjs => _lightProjs;
		public Matrix[] LightViewProjs => _lightViewProjs;

		public int Count => _distances.Length;

		public void Update(Camera camera, BaseLight light)
		{
			// Calculate split depths based on view camera frustum
			// Based on method presented in https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch10.html
			var ratio = camera.FarPlaneDistance / camera.NearPlaneDistance;
			var range = camera.FarPlaneDistance - camera.NearPlaneDistance;
			for (var i = 0; i < Count; ++i)
			{
				var p = (float)(i + 1) / Count;
				var log = camera.NearPlaneDistance * MathF.Pow(ratio, p);
				var uniform = camera.NearPlaneDistance + range * p;
				var d = Constants.ShadowMapCascadeSplitLambda * (log - uniform) + uniform;

				_distances[i] = (d - camera.NearPlaneDistance);
			}

			// Second run: calculate light cameras
			var nearPlane = camera.NearPlaneDistance;
			for (var i = 0; i < Count; ++i)
			{
				var farPlane = _distances[i];

				var proj = camera.CalculateProjection(nearPlane, farPlane);

				_sourceViewProjs[i] = camera.View * proj;

				light.GetLightViewProj(_sourceViewProjs[i], out _lightViews[i], out _lightProjs[i]);

				_lightViewProjs[i] = _lightViews[i] * _lightProjs[i];

				// Move to the next cascade
				nearPlane = farPlane;
			}
		}
	}
}
