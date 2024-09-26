using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Serialization;
using System.ComponentModel;

namespace Nursia.Standard
{
	public abstract class MeshNodeBase : SceneNode
	{
		protected abstract Mesh RenderMesh { get; }

		[Category("Appearance")]
		public IMaterial Material { get; set; }

		[DefaultValue(true)]
		[Category("Behavior")]
		public bool CastsShadow { get; set; } = true;

		[Browsable(false)]
		[JsonIgnore]
		public BoundingBox BoundingBox => RenderMesh.BoundingBox;

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			if (Material == null || RenderMesh == null)
			{
				return;
			}

			batch.BatchJob(Material, GlobalTransform, RenderMesh);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			var hasExternalAssets = Material as IHasExternalAssets;
			if (hasExternalAssets != null)
			{
				hasExternalAssets.Load(assetManager);
			}
		}
	}
}
