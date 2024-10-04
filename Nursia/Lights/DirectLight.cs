using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Lights
{
	[EditorInfo("Light")]
	public class DirectLight : BaseLight
	{
		public override bool RenderCastsShadow => CastsShadow;

		[DefaultValue(true)]
		public bool CastsShadow { get; set; } = true;

		[Browsable(false)]
		[JsonIgnore]
		public Vector3 Direction
		{
			get
			{
				var tr = GlobalTransform;

				var result = tr.Forward;
				result.Normalize();

				return result;
			}
		}

		public override void GetLightViewProj(Matrix viewProj, out Matrix view, out Matrix proj)
		{
			// Matrix with that will rotate in points the direction of the light
			Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
													   Direction,
													   Vector3.Up);

			// Get the corners of the frustum
			// Limit the camera far plance
			var cameraFrustum = new BoundingFrustum(viewProj);
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

			// The position of the light should be in the center of the box. 
			Vector3 lightPosition = lightBox.Min + halfBoxSize;
			// lightPosition.Z = lightBox.Min.Z;

			// We need the position back in world coordinates so we transform 
			// the light position by the inverse of the lights rotation
			lightPosition = Vector3.Transform(lightPosition,
											  Matrix.Invert(lightRotation));

			// Create the view matrix for the light
			view = Matrix.CreateLookAt(lightPosition,
				lightPosition + Direction,
				Vector3.Up);

			// Create the projection matrix for the light
			// The projection is orthographic since we are using a directional light
			proj = Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z * 4, boxSize.Z * 4);
		}

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			batch.DirectLights.Add(this);
		}
	}
}
