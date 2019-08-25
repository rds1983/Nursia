using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public partial class Sprite3D
	{
		private readonly List<Mesh> _meshes = new List<Mesh>();
		private readonly List<Material> _materials = new List<Material>();
		private readonly List<Bone> _bones = new List<Bone>();
		private Matrix[] _boneTransforms = null;

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

		public List<Bone> Bones
		{
			get
			{
				return _bones;
			}
		}

		public Bone RootBone { get; set; }

		private static void TraverseBones(Bone root, Action<Bone> action)
		{
			if (root == null)
			{
				return;
			}

			action(root);

			foreach(var child in root.Children)
			{
				TraverseBones(child, action);
			}
		}

		private void TraverseBones(Action<Bone> action)
		{
			TraverseBones(RootBone, action);
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null ||
				_boneTransforms.Length != Bones.Count + 1)
			{
				_boneTransforms = new Matrix[Bones.Count + 1];
			}

			_boneTransforms[0] = Matrix.Identity;
			for(var i = 0; i < Bones.Count; ++i)
			{
				_boneTransforms[i + 1] = Bones[i].AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}
