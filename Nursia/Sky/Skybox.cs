using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Primitives;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Sky
{
	public class Skybox : SceneNode
	{
		private class SkyboxMaterial : Material
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

		private readonly Mesh _meshData;

		[Browsable(false)]
		[JsonIgnore]
		public Mesh MeshData => _meshData;

		[Browsable(false)]
		[JsonIgnore]
		public TextureCube Texture
		{
			get => Material.Texture;
			set => Material.Texture = value;
		}

		[Browsable(false)]
		public string TexturePath { get; set; }

		private SkyboxMaterial Material { get; } = new SkyboxMaterial();

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
			var transform = GlobalTransform * view * context.Projection;

			// Set to the material
			Material.Transform = transform;

			context.BatchJob(Material, GlobalTransform, MeshData);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Texture = assetManager.LoadTextureCube(Nrs.GraphicsDevice, TexturePath);
		}
	}
}