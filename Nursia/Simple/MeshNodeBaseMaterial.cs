using Nursia.Rendering;

namespace Nursia.Simple
{
	public abstract class MeshNodeBaseMaterial: MeshNodeBase
	{
		public Material Material { get; set; }

		protected override Material RenderMaterial => Material;
	}
}
