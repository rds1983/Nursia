using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;

namespace Nursia.Standard
{
	public class ColorEffectBinding : EffectBinding
	{
		public EffectParameter ColorParameter { get; private set; }

		protected override void BindParameters()
		{
			base.BindParameters();

			ColorParameter = Effect.FindParameterByName("_color");
		}
	}

	public class ColorMaterial : BaseMaterial<ColorEffectBinding>, IMaterial
	{
		public Color Color { get; set; }

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		public void SetParameters()
		{
			InternalBinding.ColorParameter.SetValue(Color.ToVector4());
		}

		protected override ColorEffectBinding CreateBinding() =>
			EffectsRegistry.GetStockEffectBinding<ColorEffectBinding>("ColorEffect");
	}
}
