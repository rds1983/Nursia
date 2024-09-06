using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Simple
{
	public class MeshNode : MeshNodeBaseMaterial
	{
		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh { get; set; }

		protected override Mesh RenderMesh => Mesh;
	}
}
