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
		private class SkyboxEffectBinding : EffectBinding
		{
			private EffectParameter TextureParameter { get; set; }
			private EffectParameter TransformParameter { get; set; }

			public SkyboxEffectBinding() : base(Resources.SkyboxEffect())
			{
				TextureParameter = Effect.Parameters["_texture"];
				TransformParameter = Effect.Parameters["_transform"];
			}

			protected internal override void SetMaterialParams(IMaterial material)
			{
				base.SetMaterialParams(material);

				var skybox = (Skybox)material;

				TextureParameter.SetValue(skybox.Texture);
				TransformParameter.SetValue(skybox.Transform);
			}
		}

		private readonly Mesh _meshData;

		[Browsable(false)]
		[JsonIgnore]
		public Mesh MeshData => _meshData;

		[Browsable(false)]
		[JsonIgnore]
		public TextureCube Texture { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public Matrix Transform { get; set; }

		[Browsable(false)]
		public string TexturePath { get; set; }

		public EffectBinding DefaultEffect => new SkyboxEffectBinding();

		public EffectBinding ShadowMapEffect => null;

		public Skybox()
		{
			_meshData = PrimitiveMeshes.CubePositionFromMinusOneToOne;
		}

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			// Calculate special world-view-project matrix with zero translation
			var view = context.Camera.View;
			view.Translation = Vector3.Zero;
			Transform = GlobalTransform * view * context.Projection;

			context.BatchJob(this, GlobalTransform, MeshData);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Texture = assetManager.LoadTextureCube(Nrs.GraphicsDevice, TexturePath);
		}

		public void SetMaterialParameters(EffectBinding effectBinding)
		{
			throw new System.NotImplementedException();
		}
	}
}