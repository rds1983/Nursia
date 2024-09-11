using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Standard
{
	public abstract class MeshNodeBase : SceneNode, ICastsShadow
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

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Material == null || RenderMesh == null)
			{
				return;
			}

			context.BatchJob(this, Material, GlobalTransform, RenderMesh);
		}
	}
}
