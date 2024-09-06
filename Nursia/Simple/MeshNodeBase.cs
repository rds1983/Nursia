using Nursia.Rendering;

namespace Nursia.Simple
{
	public abstract class MeshNodeBase : SceneNode
	{
		protected abstract Mesh RenderMesh { get; }

		protected abstract Material RenderMaterial { get; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (RenderMaterial == null || RenderMesh == null)
			{
				return;
			}

			context.BatchJob("Default", RenderMaterial, GlobalTransform, RenderMesh);
		}
	}
}
