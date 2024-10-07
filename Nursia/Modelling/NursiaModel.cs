using System;
using System.Collections.Generic;

namespace Nursia.Modelling
{
	public class NursiaModel
	{
		public NursiaModelBone[] Bones { get; }
		public NursiaModelMesh[] Meshes { get; }
		public NursiaModelBone Root { get; }
		public Skin Skin { get; set; }

		public Dictionary<string, ModelAnimation> Animations { get; } = new Dictionary<string, ModelAnimation>();

		internal NursiaModel(NursiaModelBone[] bones, NursiaModelMesh[] meshes, int rootIndex = 0)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no bones");
			}

			if (meshes == null)
			{
				throw new ArgumentNullException(nameof(meshes));
			}

			if (meshes.Length == 0)
			{
				throw new ArgumentException(nameof(meshes), "no meshes");
			}

			if (rootIndex < 0 || rootIndex >= bones.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(rootIndex));
			}

			Bones = bones;
			Meshes = meshes;
			Root = bones[rootIndex];
		}

		private void TraverseNodes(NursiaModelBone root, Action<NursiaModelBone> action)
		{
			action(root);

			foreach (var child in root.Children)
			{
				TraverseNodes(child, action);
			}
		}

		public void TraverseNodes(Action<NursiaModelBone> action)
		{
			TraverseNodes(Root, action);
		}
	}
}