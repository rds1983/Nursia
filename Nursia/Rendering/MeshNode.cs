using Microsoft.Xna.Framework;

namespace Nursia.Rendering
{
	public class MeshNode : SceneNode
	{
		public Mesh Mesh { get; set; }
		public Material Material { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			context.BatchJob("Default", Material, Transform, Mesh);
		}
	}
}
