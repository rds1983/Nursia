using Microsoft.Xna.Framework;

namespace Nursia.Rendering
{
	public class OrthographicCamera : Camera
	{
		public float Width { get; set; }
		public float Height { get; set; }

		public override Matrix CalculateProjection(float near, float far)
		{
			return Matrix.CreateOrthographic(Width, Height, near, far);
		}

		public override Camera Clone()
		{
			return new OrthographicCamera
			{
				Position = Position,
				YawAngle = YawAngle,
				PitchAngle = PitchAngle,
				RollAngle = RollAngle,
				Width = Width,
				Height = Height,
				NearPlaneDistance = NearPlaneDistance,
				FarPlaneDistance = FarPlaneDistance
			};
		}
	}
}
