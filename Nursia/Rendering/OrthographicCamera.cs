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

		public new OrthographicCamera Clone() => (OrthographicCamera)base.Clone();

		protected override SceneNode CreateInstanceCore() => new OrthographicCamera();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var camera = (OrthographicCamera)node;
			Width = camera.Width;
			Height = camera.Height;
		}
	}
}
