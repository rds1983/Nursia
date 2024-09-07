using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Utilities;
using System.ComponentModel;

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

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding DefaultEffect { get; } = new ColorEffectBinding();

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding ShadowMapEffect => null;
	}
}
