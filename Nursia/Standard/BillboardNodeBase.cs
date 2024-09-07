using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;
using Plane = Nursia.Primitives.Plane;

namespace Nursia.Standard
{
	public abstract class BillboardNodeBase : SceneNode, IMaterial
	{
		private class BillboardEffectBinding : EffectBinding
		{
			private EffectParameter WidthParameter { get; set; }
			private EffectParameter HeightParameter { get; set; }
			private EffectParameter ColorParameter { get; set; }
			private EffectParameter TextureParameter { get; set; }

			public BillboardEffectBinding(bool hasTexture) :
				base(Resources.GetBillboardEffect(hasTexture)())
			{
				WidthParameter = Effect.Parameters["_width"];
				HeightParameter = Effect.Parameters["_height"];
				ColorParameter = Effect.Parameters["_color"];
				TextureParameter = Effect.Parameters["_texture"];
			}

			protected internal override void SetMaterialParams(IMaterial material)
			{
				base.SetMaterialParams(material);

				var billboard = (BillboardNodeBase)material;

				WidthParameter.SetValue(billboard.Width);
				HeightParameter.SetValue(billboard.Height);
				ColorParameter.SetValue(billboard.Color.ToVector4());
				TextureParameter?.SetValue(billboard.RenderTexture);
			}
		}

		private static Mesh _mesh;

		private BillboardEffectBinding _default = null;

		protected internal float Width { get; set; } = 1.0f;
		protected internal float Height { get; set; } = 1.0f;
		protected internal abstract Texture2D RenderTexture { get; }

		public Color Color { get; set; } = Color.White;

		private static Mesh Mesh
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

		public EffectBinding DefaultEffect
		{
			get
			{
				if (_default == null)
				{
					_default = new BillboardEffectBinding(RenderTexture != null);
				}

				return _default;
			}
		}

		public EffectBinding ShadowMapEffect => null;

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			context.BatchJob(this, GlobalTransform, Mesh);
		}

		protected void InvalidateDefault()
		{
			_default = null;
		}
	}
}
