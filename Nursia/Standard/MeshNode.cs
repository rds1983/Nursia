using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Standard
{
	public class MeshNode : MeshNodeBase
	{
		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh { get; set; }

		protected override Mesh RenderMesh => Mesh;
	}
}
