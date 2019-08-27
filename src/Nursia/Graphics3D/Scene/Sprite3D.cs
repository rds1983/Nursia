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

		public BoneNode RootNode
		{
			get; set;
		}

		private static void TraverseBoneNodes(BoneNode root, Action<BoneNode> action)
		{
			if (root == null)
			{
				return;
			}

			action(root);

			foreach (var child in root.Children)
			{
				TraverseBoneNodes(child, action);
			}
		}

		internal void TraverseBoneNodes(Action<BoneNode> action)
		{
			TraverseBoneNodes(RootNode, action);
		}

		private void UpdateBoneNodesAbsoluteTransforms(BoneNode root, Matrix transform)
		{
			if (root == null)
			{
				return;
			}

			transform = root.Transform * transform;
			root.AbsoluteTransform = transform;

			foreach (var child in root.Children)
			{
				UpdateBoneNodesAbsoluteTransforms(child, transform);
			}
		}

		internal void UpdateBoneNodesAbsoluteTransforms()
		{
			if (RootNode == null)
			{
				return;
			}

			UpdateBoneNodesAbsoluteTransforms(RootNode, Matrix.Identity);
		}
	}
}