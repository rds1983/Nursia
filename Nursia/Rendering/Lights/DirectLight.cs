using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
{
	public class DirectLight : BaseLight
	{
		private Vector3 _direction;
		private Vector3 _normalizedDirection;
		private readonly OrthographicCamera _camera = new OrthographicCamera();

		public override bool RenderCastsShadow => CastsShadow;

		[DefaultValue(true)]
		public bool CastsShadow { get; set; } = true;

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
		}

		public override Camera GetLightCamera(Camera camera)
		{
			// Matrix with that will rotate in points the direction of the light
			Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
													   NormalizedDirection,
													   Vector3.Up);

			// Get the corners of the frustum
			// Limit the camera far plance
			var cameraProjection = camera.CalculateProjection();
			var cameraFrustum = new BoundingFrustum(camera.View * cameraProjection);
			Vector3[] frustumCorners = cameraFrustum.GetCorners();

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
			// lightPosition.Z = lightBox.Min.Z;

			// We need the position back in world coordinates so we transform 
			// the light position by the inverse of the lights rotation
			lightPosition = Vector3.Transform(lightPosition,
											  Matrix.Invert(lightRotation));

			// Create the view matrix for the light
			_camera.SetLookAt(lightPosition, lightPosition + NormalizedDirection);

			// Create the projection matrix for the light
			// The projection is orthographic since we are using a directional light
			_camera.Width = boxSize.X;
			_camera.Height = boxSize.Y;
			_camera.NearPlaneDistance = -boxSize.Z;
			_camera.FarPlaneDistance = boxSize.Z;

			return _camera;
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

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			batch.DirectLights.Add(this);
		}
	}
}
