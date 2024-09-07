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

		public override Matrix CreateLightViewProjectionMatrix(RenderContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
