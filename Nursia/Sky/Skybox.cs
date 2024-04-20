using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Sky
{
	public class Skybox : SceneNode
	{
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
			var transform = Transform * view * context.Projection;

			// Set to the material
			Material.Transform = transform;

			context.BatchJob("Default", Material, Transform, MeshData);
		}

		protected internal override void LoadResources(AssetManager assetManager)
		{
			base.LoadResources(assetManager);

			var res = ExternalResources["Texture"];

			var device = Nrs.GraphicsDevice;
			Texture = assetManager.LoadTextureCube(Nrs.GraphicsDevice, res);
		}
	}
}