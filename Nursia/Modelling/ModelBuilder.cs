using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nursia.Modelling
{
	/// <summary>
	/// Model bone descriptor
	/// </summary>
	public class NursiaModelBoneDesc
	{
		/// <summary>
		/// Name of the model bone
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Translaction
		/// </summary>
		public Vector3 Translation = Vector3.Zero;

		/// <summary>
		/// Scale
		/// </summary>
		public Vector3 Scale = Vector3.One;

		/// <summary>
		/// Rotation
		/// </summary>
		public Quaternion Rotation = Quaternion.Identity;

		/// <summary>
		/// Meshes of the model bone
		/// </summary>
		public readonly List<NursiaModelMesh> Meshes = new List<NursiaModelMesh>();

		/// <summary>
		/// Children of the model bone
		/// </summary>
		public readonly List<NursiaModelBoneDesc> Children = new List<NursiaModelBoneDesc>();

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
		/// <param name="rootBoneIndex">Index of the root node</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static NursiaModel Create(List<NursiaModelBoneDesc> bones, List<NursiaModelMesh> meshes, int rootBoneIndex = 0)
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
					DefaultTranslation = desc.Translation,
					DefaultScale = desc.Scale,
					DefaultRotation = desc.Rotation,
					Meshes = desc.Meshes.ToArray(),
				};

				allBones.Add(bone);
			}

			// Assign children
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = allBones[i];

				var childrenArray = (from c in desc.Children select allBones[c.Index]).ToArray();
				bone.Children = childrenArray;
			}

			// Create the model
			return new NursiaModel(allBones.ToArray(), meshes.ToArray(), rootBoneIndex);
		}
	}
}
