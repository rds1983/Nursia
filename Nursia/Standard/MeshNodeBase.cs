using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Standard
{
	public abstract class MeshNodeBase : SceneNode
	{
		protected abstract Mesh RenderMesh { get; }

		public IMaterial Material { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Material == null || RenderMesh == null)
			{
				return;
			}

			context.BatchJob(Material, GlobalTransform, RenderMesh);
		}
	}
}
