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

		public override Camera GetLightCamera(Vector3 viewerPos)
		{
			throw new System.NotImplementedException();
		}
	}
}
