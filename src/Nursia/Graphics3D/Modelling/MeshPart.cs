using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class MeshPart
	{
		private readonly List<Bone> _bones = new List<Bone>();
		private Matrix[] _boneTransforms = null;

		internal string MeshPartId { get; set; }
		internal string MaterialId { get; set; }

		public Material Material { get; set; }
		public Mesh Mesh { get; set; }

		public BonesPerMesh BonesPerMesh { get; set; }

		public BoundingSphere BoundingSphere { get; set; }

		public Matrix Transform;

		public List<Bone> Bones
		{
			get
			{
				return _bones;
			}
		}

		public MeshPart()
		{
			Transform = Matrix.Identity;
		}

		internal Matrix[] CalculateBoneTransforms()
		{
			if (_boneTransforms == null ||
				_boneTransforms.Length != Bones.Count)
			{
				_boneTransforms = new Matrix[Bones.Count];
			}

			for (var i = 0; i < Bones.Count; ++i)
			{
				_boneTransforms[i] = Bones[i].Transform * Bones[i].ParentNode.AbsoluteTransform;
			}

			return _boneTransforms;
		}
	}
}
