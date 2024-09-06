using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;

namespace Nursia.Simple
{
	public class BillboardNode : MeshNodeBase
	{
		private class BillboardMaterial : Material
		{
			public Color Color { get; set; } = Color.White;
			
			private EffectParameter ColorParameter { get; set; }


			protected override Effect CreateEffect()
			{
				var effect = Resources.BillboardEffect();
				ColorParameter = effect.Parameters["_color"];

				return effect;
			}

			protected internal override void SetMaterialParameters()
			{
				base.SetMaterialParameters();

				ColorParameter.SetValue(Color.ToVector4());
			}
		}

		private readonly BillboardMaterial _material = new BillboardMaterial();
		private readonly Mesh _mesh;

		protected override Material RenderMaterial => _material;
		protected override Mesh RenderMesh => _mesh;

		public Color Color
		{
			get => _material.Color;
			set => _material.Color = value;
		}

		public BillboardNode()
		{
			var vertices = new VertexPositionTexture[]
			{
				new VertexPositionTexture(Vector3.Zero, new Vector2(0, 0)),
				new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0)),
				new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1)),
				new VertexPositionTexture(Vector3.Zero, new Vector2(1, 1)),
			};

			var indices = new short[] { 0, 1, 2, 2, 1, 3 };

			_mesh = new Mesh(vertices, indices);
		}

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);
		}
	}
}
