using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia.Rendering
{
	public class ColorMaterial : Material
	{
		public Color Color { get; set; }

		private EffectParameter ColorParameter { get; set; }


		protected override Effect CreateEffect()
		{
			var result = Resources.ColorEffect();

			ColorParameter = result.FindParameterByName("_color");

			return result;
		}

		protected internal override void SetMaterialParameters()
		{
			base.SetMaterialParameters();

			ColorParameter.SetValue(Color.ToVector4());
		}
	}
}
