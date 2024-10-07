using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nursia.Modelling
{
	public class SkinJoint
	{
		public NursiaModelBone Bone { get; }
		public Matrix InverseBindTransform { get; }

		public SkinJoint(NursiaModelBone bone, Matrix inverseBindTransform)
		{
			if (bone == null)
			{
				throw new ArgumentNullException(nameof(bone));
			}

			Bone = bone;
			InverseBindTransform = inverseBindTransform;
		}
	}

	public class Skin : ItemWithId
	{
		public SkinJoint[] Joints { get; }

		/// <summary>
		/// Creates a skin from array of joints
		/// </summary>
		/// <param name="joints"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public Skin(SkinJoint[] joints)
		{
			if (joints == null)
			{
				throw new ArgumentNullException(nameof(joints));
			}

			if (joints.Length == 0)
			{
				throw new ArgumentException(nameof(joints), "no joints");
			}

			Joints = joints;
		}

		/// <summary>
		/// Creates a skin from array of bones,
		/// automatically calculating inverse bind matrices by inverting bones' absolute transforms.
		/// </summary>
		/// <param name="bones"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public Skin(NursiaModelBone[] bones)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no joints");
			}

			var joints = new List<SkinJoint>();
			foreach (var bone in bones)
			{
				var inverseBindTransform = Matrix.Invert(bone.CalculateDefaultAbsoluteTransform());
				joints.Add(new SkinJoint(bone, inverseBindTransform));
			}

			Joints = joints.ToArray();
		}
	}
}