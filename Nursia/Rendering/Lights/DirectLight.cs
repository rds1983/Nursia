using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
{
	public class DirectLight : BaseLight
	{
		private const int ShadowMapSize = 2048;

		private Vector3[] _boundingBoxCorners = new Vector3[8];

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

		public BoundingBox SceneBoundingBox { get; private set; }


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
			// Matrix with that will rotate in points the direction of the light
			var lightRotation = Matrix.CreateLookAt(Vector3.Zero, NormalizedDirection, Vector3.Up);

			// Calculate total bounding box
			var lightBox = new BoundingBox();
			foreach (var job in context.Jobs)
			{
				if (job.Mesh == null || !job.Mesh.CastsShadow)
				{
					continue;
				}

				var m = job.Transform;
				var meshBoundingBox = job.Mesh.BoundingBox.Transform(ref m);

				lightBox = BoundingBox.CreateMerged(lightBox, meshBoundingBox);
			}

			// Apply rotating to the bounding box
			lightBox.GetCorners(_boundingBoxCorners);

			for(var i = 0; i < _boundingBoxCorners.Length; i++)
			{
				Vector3 v;
				Vector3.Transform(ref _boundingBoxCorners[i], ref lightRotation, out v);

				_boundingBoxCorners[i] = v;
			}

			// Find the smallest box around the points
			lightBox = BoundingBox.CreateFromPoints(_boundingBoxCorners);

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
												   lightPosition + NormalizedDirection,
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
