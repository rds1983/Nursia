using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;
using Plane = Nursia.Primitives.Plane;

namespace Nursia.Standard
{
	public abstract class BillboardNodeBase : MeshNodeBase
	{
		private class BillboardMaterial : Material
		{
			private Texture2D _texture;

			public float Width { get; set; } = 1.0f;
			public float Height { get; set; } = 1.0f;

			public Color Color { get; set; } = Color.White;

			[Browsable(false)]
			[JsonIgnore]
			public Texture2D Texture
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

			private EffectParameter WidthParameter { get; set; }
			private EffectParameter HeightParameter { get; set; }
			private EffectParameter ColorParameter { get; set; }
			private EffectParameter TextureParameter { get; set; }


			protected override Effect CreateEffect()
			{
				var effect = Resources.GetBillboardEffect(Texture != null)();

				WidthParameter = effect.Parameters["_width"];
				HeightParameter = effect.Parameters["_height"];
				ColorParameter = effect.Parameters["_color"];
				TextureParameter = effect?.Parameters["_texture"];

				return effect;
			}

			protected internal override void SetMaterialParameters()
			{
				base.SetMaterialParameters();

				WidthParameter.SetValue(Width);
				HeightParameter.SetValue(Height);
				ColorParameter.SetValue(Color.ToVector4());
				TextureParameter?.SetValue(Texture);
			}
		}

		private readonly BillboardMaterial _material = new BillboardMaterial();
		private static Mesh _mesh;

		protected override Material RenderMaterial => _material;
		protected override Mesh RenderMesh
		{
			get
			{
				if (_mesh == null)
				{
					var plane = new Plane();
					_mesh = plane.Mesh;
				}

				return _mesh;
			}
		}

		public Color Color
		{
			get => _material.Color;
			set => _material.Color = value;
		}

		protected internal float Width
		{
			get => _material.Width;
			set => _material.Width = value;
		}

		protected internal float Height
		{
			get => _material.Height;
			set => _material.Height = value;
		}

		protected abstract internal Texture2D RenderTexture { get; }

		protected internal override void BeforeRender(RenderContext context)
		{
			base.BeforeRender(context);

			_material.Texture = RenderTexture;
		}
	}
}
