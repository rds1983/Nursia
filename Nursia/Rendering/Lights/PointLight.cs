using Microsoft.Xna.Framework;

namespace Nursia.Rendering.Lights
{
	public class PointLight : BaseLight
	{
		public override bool RenderCastsShadow => false;

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			batch.PointLights.Add(this);
		}

		public override Camera GetLightCamera(Camera camera)
		{
			throw new System.NotImplementedException();
		}
	}
}
