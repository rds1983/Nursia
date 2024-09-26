using AssetManagementBase;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Modelling
{
	public class NursiaModel : SceneNode
	{
		[Browsable(false)]
		[JsonIgnore]
		public ModelInstance Model { get; set; }

		[Browsable(false)]
		public string ModelPath { get; set; }

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			if (Model == null)
			{
				return;
			}

			Model.UpdateNodesAbsoluteTransforms();

			var transform = GlobalTransform;
			foreach (var node in Model.RootNodes)
			{
				node.Render(this, batch, ref transform);
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Model = assetManager.LoadGltf(ModelPath);
		}
	}
}
