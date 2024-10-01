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

		public override void GetLightViewProj(Matrix viewProj, out Matrix view, out Matrix proj)
		{
			throw new System.NotImplementedException();
		}
	}
}
