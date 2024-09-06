using Nursia.Rendering;

namespace Nursia.Standard
{
	public abstract class MeshNodeBaseMaterial: MeshNodeBase
	{
		public Material Material { get; set; }

		protected override Material RenderMaterial => Material;
	}
}
