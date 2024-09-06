using Nursia.Rendering;

namespace Nursia.Standard
{
	public abstract class MeshNodeBase : SceneNode
	{
		protected abstract Mesh RenderMesh { get; }

		protected abstract Material RenderMaterial { get; }

		protected internal virtual void BeforeRender(RenderContext context)
		{
		}

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			BeforeRender(context);

			if (RenderMaterial == null || RenderMesh == null)
			{
				return;
			}

			context.BatchJob("Default", RenderMaterial, GlobalTransform, RenderMesh);
		}
	}
}
