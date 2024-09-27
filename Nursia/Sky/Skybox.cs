using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Primitives;
using Nursia.Rendering;
using Nursia.Standard;
using System.ComponentModel;

namespace Nursia.Sky
{
	public class SkyboxEffectBinding: EffectBinding
	{
		public EffectParameter TextureParameter { get; private set; }
		public EffectParameter TransformParameter { get; private set; }

		protected override void BindParameters()
		{
			base.BindParameters();

			TextureParameter = Effect.Parameters["_texture"];
			TransformParameter = Effect.Parameters["_transform"];
		}
	}

	public class Skybox : SceneNode, IMaterial
	{
		private static readonly SkyboxEffectBinding _binding =
			EffectsRegistry.GetStockEffectBinding<SkyboxEffectBinding>("SkyboxEffect");
		private readonly Mesh _mesh;

		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh => _mesh;

		[Browsable(false)]
		[JsonIgnore]
		public TextureCube Texture { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public Matrix Transform { get; set; }

		[Browsable(false)]
		public string TexturePath { get; set; }

		public EffectBinding EffectBinding => _binding;

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		public Skybox()
		{
			var effect = EffectBinding.Effect;

			_mesh = PrimitiveMeshes.CubePositionFromMinusOneToOne;
		}

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			// Calculate special world-view-project matrix with zero translation
			var view = batch.View;
			view.Translation = Vector3.Zero;
			Transform = GlobalTransform * view * batch.Projection;

			batch.BatchJob(this, GlobalTransform, Mesh);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Texture = assetManager.LoadTextureCube(Nrs.GraphicsDevice, TexturePath);
		}

		public void SetParameters()
		{
			_binding.TextureParameter.SetValue(Texture);
			_binding.TransformParameter.SetValue(Transform);
		}
	}
}