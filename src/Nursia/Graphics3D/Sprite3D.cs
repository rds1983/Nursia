using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public partial class Sprite3D
	{
		private readonly List<Mesh> _meshes = new List<Mesh>();

		public List<Mesh> Meshes
		{
			get
			{
				return _meshes;
			}
		}
	}
}
