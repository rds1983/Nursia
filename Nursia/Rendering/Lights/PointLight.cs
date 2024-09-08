using Microsoft.Xna.Framework;

namespace Nursia.Rendering.Lights
{
	public class PointLight : BaseLight
	{
		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			context.PointLights.Add(this);
		}

		public override Matrix CreateLightViewProjectionMatrix(Camera camera)
		{
			throw new System.NotImplementedException();
		}
	}
}
