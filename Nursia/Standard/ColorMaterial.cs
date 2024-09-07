using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;

namespace Nursia.Standard
{
    public class ColorMaterial : IMaterial
    {
        private class ColorEffectBinding : EffectBinding
        {
            private EffectParameter ColorParameter { get; set; }

            public ColorEffectBinding() : base(Resources.ColorEffect())
            {
                ColorParameter = Effect.FindParameterByName("_color");
            }

            protected internal override void SetMaterialParams(IMaterial material)
            {
                base.SetMaterialParams(material);

                var colorMaterial = (ColorMaterial)material;
                ColorParameter.SetValue(colorMaterial.Color.ToVector4());
            }
        }

        public Color Color { get; set; }

        public EffectBinding DefaultEffect { get; } = new ColorEffectBinding();

        public EffectBinding ShadowMapEffect => null;
    }
}
