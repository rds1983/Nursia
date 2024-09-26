using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Primitives;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Sky
{
	public class Skybox : SceneNode, IMaterial
	{
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

		public EffectBinding EffectBinding => Resources.SkyboxEffectBinding();

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		private EffectParameter TextureParameter { get; set; }
		private EffectParameter TransformParameter { get; set; }


		public Skybox()
		{
			var effect = EffectBinding.Effect;
			TextureParameter = effect.Parameters["_texture"];
			TransformParameter = effect.Parameters["_transform"];

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

		public void SetParameters(Mesh mesh)
		{
			TextureParameter.SetValue(Texture);
			TransformParameter.SetValue(Transform);
		}
	}
}