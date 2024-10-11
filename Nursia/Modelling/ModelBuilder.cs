using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nursia.Modelling
{
	public class SkinJointDesc
	{
		public int BoneIndex { get; set; }
		public Matrix InverseBindTransform { get; set; }
	}

	public class SkinDesc
	{
		public readonly List<SkinJointDesc> Joints = new List<SkinJointDesc>();
	}

	/// <summary>
	/// Model bone descriptor
	/// </summary>
	public class NursiaModelBoneDesc
	{
		/// <summary>
		/// Name of the model bone
		/// </summary>
		public string Name { get; set; }

		public Pose Pose = Pose.Identity;

		/// <summary>
		/// Meshes of the model bone
		/// </summary>
		public readonly List<NursiaModelMesh> Meshes = new List<NursiaModelMesh>();

		/// <summary>
		/// Children of the model bone
		/// </summary>
		public readonly List<int> ChildrenIndices = new List<int>();

		/// <summary>
		/// Skin of the bone meshes
		/// </summary>
		public int? SkinIndex { get; set; }

		internal int Index { get; set; }
	}

	/// <summary>
	/// Grants ability to create a Model at the run-time
	/// </summary>
	public static class NursiaModelBuilder
	{
		/// <summary>
		/// Creates the model
		/// </summary>
		/// <param name="meshes">Meshes of the model</param>
		/// <param name="bones">Bones of the model</param>
		/// <param name="skins">Skins of the model</param>
		/// <param name="rootBoneIndex">Index of the root node</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static NursiaModel Create(List<NursiaModelBoneDesc> bones, List<NursiaModelMesh> meshes, List<SkinDesc> skins, int rootBoneIndex = 0)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Count == 0)
			{
				throw new ArgumentException(nameof(bones), "no bones");
			}

			if (meshes == null)
			{
				throw new ArgumentNullException(nameof(meshes));
			}

			if (meshes.Count == 0)
			{
				throw new ArgumentException(nameof(meshes), "no meshes");
			}

			if (rootBoneIndex < 0 || rootBoneIndex >= bones.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rootBoneIndex));
			}

			// Assign indexes
			for (var i = 0; i < bones.Count; ++i)
			{
				bones[i].Index = i;
			}

			// Create bones
			var allBones = new List<NursiaModelBone>();
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = new NursiaModelBone(i, desc.Name)
				{
					DefaultPose = desc.Pose,
					Meshes = desc.Meshes.ToArray(),
				};

				allBones.Add(bone);
			}

			// Create skins
			List<Skin> allSkins = null;
			if (skins != null && skins.Count > 0)
			{
				allSkins = new List<Skin>();
				for (var i = 0; i < skins.Count; ++i)
				{
					var skin = new Skin((from j in skins[i].Joints select new SkinJoint(allBones[j.BoneIndex], j.InverseBindTransform)).ToArray())
					{
						SkinIndex = i
					};

					allSkins.Add(skin);
				}
			}

			// Assign children and skins
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = allBones[i];

				var childrenArray = (from c in desc.ChildrenIndices select allBones[c]).ToArray();
				bone.Children = childrenArray;

				if (desc.SkinIndex != null)
				{
					bone.Skin = allSkins[desc.SkinIndex.Value];
				}
			}

			// Create the model
			return new NursiaModel(allBones.ToArray(), meshes.ToArray(), allSkins != null ? allSkins.ToArray() : null, rootBoneIndex);
		}
	}
}
