using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public partial class Sprite3D
	{
		private readonly List<Mesh> _meshes = new List<Mesh>();
		private readonly List<Material> _materials = new List<Material>();

		public Matrix Transform;

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

		private static void TraverseBones(Bone root, Action<Bone> action)
		{
			if (root == null)
			{
				return;
			}

			action(root);

			foreach (var child in root.Children)
			{
				TraverseBones(child, action);
			}
		}
	}
}