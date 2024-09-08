using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
{
	public class DirectLight : BaseLight
	{
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

		public override Matrix CreateLightViewProjectionMatrix(RenderContext context)
		{
			Vector3 lightDir = -NormalizedDirection;

			// Matrix with that will rotate in points the direction of the light
			Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
													   -lightDir,
													   Vector3.Up);

			// Get the corners of the frustum
			Vector3[] frustumCorners = context.Frustum.GetCorners();

			// Transform the positions of the corners into the direction of the light
			for (int i = 0; i < frustumCorners.Length; i++)
			{
				frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
			}

			// Find the smallest box around the points
			BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

			Vector3 boxSize = lightBox.Max - lightBox.Min;
			Vector3 halfBoxSize = boxSize * 0.5f;

			// The position of the light should be in the center of the back
			// pannel of the box. 
			Vector3 lightPosition = lightBox.Min + halfBoxSize;
			lightPosition.Z = lightBox.Min.Z;

			// We need the position back in world coordinates so we transform 
			// the light position by the inverse of the lights rotation
			lightPosition = Vector3.Transform(lightPosition,
											  Matrix.Invert(lightRotation));

			// Create the view matrix for the light
			Matrix lightView = Matrix.CreateLookAt(lightPosition,
												   lightPosition - lightDir,
												   Vector3.Up);

			// Create the projection matrix for the light
			// The projection is orthographic since we are using a directional light
			Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
															   -boxSize.Z, boxSize.Z);

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
