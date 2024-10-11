using System;
using System.Collections.Generic;
using Nursia.Animation;

namespace Nursia.Modelling
{
	public class NursiaModel
	{
		public NursiaModelBone[] Bones { get; }
		public NursiaModelMesh[] Meshes { get; }
		public Skin[] Skins { get; }
		public NursiaModelBone Root { get; }

		public Dictionary<string, AnimationClip> Animations { get; } = new Dictionary<string, AnimationClip>();

		internal NursiaModel(NursiaModelBone[] bones, NursiaModelMesh[] meshes, Skin[] skins, int rootIndex = 0)
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
			Skins = skins;
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