using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public partial class Sprite3D
	{
		private readonly List<Mesh> _meshes = new List<Mesh>();
		private readonly List<Material> _materials = new List<Material>();

		public List<Mesh> Meshes
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
