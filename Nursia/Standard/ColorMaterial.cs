using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;

namespace Nursia.Standard
{
	public class ColorMaterial : IMaterial
	{
		private EffectBinding _effectBinding = Resources.ColorEffectBinding();

		public Color Color { get; set; }

		public EffectBinding EffectBinding => _effectBinding;

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		private EffectParameter ColorParameter { get; set; }

		public ColorMaterial()
		{
			ColorParameter = _effectBinding.Effect.FindParameterByName("_color");
		}

		public void SetParameters()
		{
			ColorParameter.SetValue(Color.ToVector4());
		}
	}
}
