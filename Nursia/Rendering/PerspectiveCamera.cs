using Microsoft.Xna.Framework;

namespace Nursia.Rendering
{
	public class PerspectiveCamera : Camera
	{
		public float ViewAngle { get; set; } = 90.0f;

		public override Matrix CalculateProjection(float near, float far) =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				Nrs.GraphicsDevice.Viewport.AspectRatio,
				near, far);

		public new PerspectiveCamera Clone() => (PerspectiveCamera)base.Clone();

		protected override SceneNode CreateInstanceCore() => new PerspectiveCamera();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var camera = (PerspectiveCamera)node;
			ViewAngle = camera.ViewAngle;
		}
	}
}
