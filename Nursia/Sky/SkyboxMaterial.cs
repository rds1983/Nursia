using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;

namespace Nursia.Sky
{
	internal class SkyboxMaterial : Material
	{
		public TextureCube _texture;

		public TextureCube Texture
		{
			get => _texture;

			set
			{
				if (value == _texture)
				{
					return;
				}

				_texture = value;
				Invalidate();
			}
		}

		public Matrix Transform;

		private EffectParameter TextureParameter { get; set; }
		private EffectParameter TransformParameter { get; set; }

		protected override Effect CreateEffect()
		{
			var effect = Resources.SkyboxEffect();

			TextureParameter = effect.Parameters["_texture"];
			TransformParameter = effect.Parameters["_transform"];

			return effect;
		}

		protected internal override void SetMaterialParameters()
		{
			base.SetMaterialParameters();

			TextureParameter.SetValue(Texture);
			TransformParameter.SetValue(Transform);
		}
	}
}
