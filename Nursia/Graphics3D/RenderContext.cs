using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Lights;

namespace Nursia.Graphics3D
{
	internal enum RenderPassType
	{
		Color,
		Depth
	}

	internal class RenderContext
	{
		private int[] _effectLightType = new int[Constants.MaxLights];
		private Vector3[] _effectLightPosition = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightDirection = new Vector3[Constants.MaxLights];
		private Vector3[] _effectLightColor = new Vector3[Constants.MaxLights];

		private readonly RenderStatistics _statistics = new RenderStatistics();
		private Matrix? _viewProjection;
		private BoundingFrustum _frustrum;
		private Matrix _projection = Matrix.Identity, _view = Matrix.Identity;

		public Scene Scene { get; set; }

		public Plane? ClipPlane;

		public List<DirectLight> DirectLights => Scene.DirectLights;
		public List<BaseLight> PointLights => Scene.PointLights;

		public bool HasLights => Scene.HasLights;

		public Matrix Projection
		{
			get
			{
				return _projection;
			}

			set
			{
				if (value == _projection)
				{
					return;
				}

				_projection = value;
				ResetView();
			}
		}

		public Matrix View
		{
			get
			{
				return _view;
			}

			set
			{
				if (value == _view)
				{
					return;
				}


				_view = value;
				ResetView();
			}
		}

		public Matrix ViewProjection
		{
			get
			{
				if (_viewProjection == null)
				{
					_viewProjection = View * Projection;
				}

				return _viewProjection.Value;
			}
		}

		public BoundingFrustum Frustrum
		{
			get
			{
				if (_frustrum == null)
				{
					_frustrum = new BoundingFrustum(ViewProjection);
				}

				return _frustrum;
			}
		}

		public float NearPlaneDistance = 0.1f;
		public float FarPlaneDistance = 1000.0f;

		public RenderPassType RenderPassType { get; set; }

		public RenderTarget2D Screen;
		public RenderTarget2D Depth;

		public RenderStatistics Statistics
		{
			get
			{
				return _statistics;
			}
		}

		private void ResetView()
		{
			_viewProjection = null;
			_frustrum = null;
		}

		public RenderContext()
		{
		}

		public void SetLights(Effect effect)
		{
			var lightIndex = 0;
			foreach (var directLight in DirectLights)
			{
				if (lightIndex >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[lightIndex] = 0;
				_effectLightColor[lightIndex] = directLight.Color.ToVector3();
				_effectLightDirection[lightIndex] = directLight.NormalizedDirection;

				++lightIndex;
			}

			foreach (var pointLight in PointLights)
			{
				if (lightIndex >= Constants.MaxLights)
				{
					break;
				}

				_effectLightType[lightIndex] = 1;
				_effectLightColor[lightIndex] = pointLight.Color.ToVector3();
				_effectLightPosition[lightIndex] = pointLight.Position;

				++lightIndex;
			}

			effect.Parameters["_lightType"].SetValue(_effectLightType);
			effect.Parameters["_lightPosition"].SetValue(_effectLightPosition);
			effect.Parameters["_lightDirection"].SetValue(_effectLightDirection);
			effect.Parameters["_lightColor"].SetValue(_effectLightColor);
			effect.Parameters["_lightCount"].SetValue(lightIndex);
		}
	}
}
