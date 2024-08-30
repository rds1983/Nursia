using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Modelling
{
	public class NursiaModel: SceneNode
	{
		[Browsable(false)]
		[JsonIgnore]
		public ModelInstance Model { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Model == null)
			{
				return;
			}

			Model.UpdateNodesAbsoluteTransforms();

			var transform = Transform;
			foreach (var node in Model.RootNodes)
			{
				node.Render(context, ref transform);
			}
		}

		public override void LoadResources(AssetManager assetManager)
		{
			base.LoadResources(assetManager);

			Model = assetManager.LoadGltf(ExternalResources["Model"]);
		}
	}
}
