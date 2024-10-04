using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Rendering;

namespace Nursia.Lights
{
    [EditorInfo("Lighs")]
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
