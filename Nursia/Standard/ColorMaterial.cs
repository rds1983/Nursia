using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;

namespace Nursia.Standard
{
	public class ColorMaterial : IMaterial
	{
		public Color Color { get; set; }

		public EffectBinding EffectBinding => DefaultEffects.ColorEffectBinding;

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		private EffectParameter ColorParameter { get; set; }

		public ColorMaterial()
		{
			ColorParameter = EffectBinding.Effect.FindParameterByName("_color");
		}

		public void SetParameters()
		{
			ColorParameter.SetValue(Color.ToVector4());
		}
	}
}
