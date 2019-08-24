using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public partial class Sprite3D: Node
	{
		private readonly List<MeshNode> _meshes = new List<MeshNode>();
		private readonly List<Material> _materials = new List<Material>();

		public List<MeshNode> Meshes
		{
			get
			{
				return _meshes;
			}
		}

		public List<Material> Materials
		{
			get
			{
				return _materials;
			}
		}
	}
}
