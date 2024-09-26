using Microsoft.Xna.Framework;

namespace Nursia.Rendering
{
	public class PerspectiveCamera : Camera
	{
		public float ViewAngle { get; set; } = 90.0f;

		public override Matrix CalculateProjection() =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				Nrs.GraphicsDevice.Viewport.AspectRatio, NearPlaneDistance, FarPlaneDistance);

		public override Camera Clone()
		{
			return new PerspectiveCamera
			{
				Position = Position,
				YawAngle = YawAngle,
				PitchAngle = PitchAngle,
				RollAngle = RollAngle,
				ViewAngle = ViewAngle,
				NearPlaneDistance = NearPlaneDistance,
				FarPlaneDistance = FarPlaneDistance
			};
		}
	}
}
