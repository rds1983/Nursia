using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
{
	public class DirectLight : BaseLight
	{
		private const int SceneRadius = 50;

		private const int ShadowMapSize = 2048;

		private Vector3 _direction;
		private Vector3 _normalizedDirection;

		public Vector3 Direction
		{
			get { return _direction; }
			set
			{
				_direction = value;
				UpdateNormalizedDirection();
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public Vector3 NormalizedDirection => _normalizedDirection;

		public DirectLight()
		{
			Direction = new Vector3(1, 0, 0);

			ShadowMap = new RenderTarget2D(Nrs.GraphicsDevice,
								ShadowMapSize, ShadowMapSize,
								false, SurfaceFormat.Single,
								DepthFormat.Depth24);
		}

		public override Matrix CreateLightViewProjectionMatrix(Camera camera)
		{
			var lightPos = camera.Position - NormalizedDirection * SceneRadius;

			var lightView = Matrix.CreateLookAt(lightPos, lightPos + NormalizedDirection, Vector3.Up);

			var lightProjection = Matrix.CreateOrthographic(SceneRadius * 2, SceneRadius * 2, 0.1f, SceneRadius * 2);

			return lightView * lightProjection;
		}

		private void UpdateNormalizedDirection()
		{
			var length = _direction.Length();
			if (length < 0.00001f)
			{
				_normalizedDirection = Vector3.Zero;
			}
			else
			{
				_normalizedDirection = _direction;
				_normalizedDirection.Normalize();
			}
		}

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			context.DirectLights.Add(this);
		}
	}
}
