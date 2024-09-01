using AssetManagementBase;
using Newtonsoft.Json;
using System.ComponentModel;
using static Nursia.Rendering.PrimitiveMeshes;

namespace Nursia.Rendering
{
	public class MeshNode : SceneNode
	{
		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh { get; set; }

		public Material Material { get; set; }

		[Browsable(false)]
		public object CreationParams { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Material == null || Mesh == null)
			{
				return;
			}

			context.BatchJob("Default", Material, Transform, Mesh);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (CreationParams == null)
			{
				return;
			}

			if (CreationParams is CubeParameters)
			{
				var asCubeParams = (CubeParameters)CreationParams;
				Mesh = CreateCube(asCubeParams);
			}

			if (CreationParams is CylinderParameters)
			{
				var asCylinderParams = (CylinderParameters)CreationParams;
				Mesh = CreateCylinder(asCylinderParams);
			}
		}
	}
}
