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
		private static Mesh _mesh;
		private EffectBinding _effectBinding;

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

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding EffectBinding
		{
			get
			{
				if (_effectBinding == null)
				{
					_effectBinding = DefaultEffects.GetBillboardEffectBinding(RenderTexture != null);

					var effect = _effectBinding.Effect;
					WidthParameter = effect.Parameters["_width"];
					HeightParameter = effect.Parameters["_height"];
					ColorParameter = effect.Parameters["_color"];
					TextureParameter = effect.Parameters["_texture"];

				}

				return _effectBinding;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding ShadowMapEffect => null;

		[DefaultValue(NodeBlendMode.Transparent)]
		public NodeBlendMode BlendMode { get; set; } = NodeBlendMode.Transparent;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		protected internal float Width { get; set; } = 1.0f;
		protected internal float Height { get; set; } = 1.0f;
		protected internal abstract Texture2D RenderTexture { get; }

		private EffectParameter WidthParameter { get; set; }
		private EffectParameter HeightParameter { get; set; }
		private EffectParameter ColorParameter { get; set; }
		private EffectParameter TextureParameter { get; set; }

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			batch.BatchJob(this, GlobalTransform, Mesh);
		}

		protected void InvalidateDefault()
		{
			_effectBinding = null;
		}

		public void SetParameters()
		{
			WidthParameter.SetValue(Width);
			HeightParameter.SetValue(Height);
			ColorParameter.SetValue(Color.ToVector4());
			TextureParameter?.SetValue(RenderTexture);
		}
	}
}
