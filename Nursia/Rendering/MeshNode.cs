using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering
{
	public class MeshNode : SceneNode
	{
		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh { get; set; }

		public Material Material { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Material == null || Mesh == null)
			{
				return;
			}

			context.BatchJob("Default", Material, GlobalTransform, Mesh);
		}
	}
}
