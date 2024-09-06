using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Primitives;
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

			public Matrix Transform;


			private EffectParameter ColorParameter { get; set; }
			private EffectParameter TextureParameter { get; set; }
			private EffectParameter TransformParameter { get; set; }


			protected override Effect CreateEffect()
			{
				var effect = Resources.GetBillboardEffect(Texture != null)();

				ColorParameter = effect.Parameters["_color"];
				TextureParameter = effect?.Parameters["_texture"];
				TransformParameter = effect.Parameters["_transform"];

				return effect;
			}

			protected internal override void SetMaterialParameters()
			{
				base.SetMaterialParameters();

				ColorParameter.SetValue(Color.ToVector4());
				TextureParameter?.SetValue(Texture);
				TransformParameter.SetValue(Transform);
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

		protected abstract internal Texture2D RenderTexture { get; }

		protected internal override void BeforeRender(RenderContext context)
		{
			base.BeforeRender(context);

			_material.Texture = RenderTexture;

			var transform = Matrix.CreateBillboard(Translation, context.Camera.Position, context.Camera.Up, context.Camera.Direction);
			_material.Transform = GlobalTransform * transform * context.ViewProjection;
		}
	}
}
