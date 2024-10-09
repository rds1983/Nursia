using glTFLoader.Schema;
using Nursia.Modelling;
using System;
using System.Collections.Generic;

namespace Nursia.Animation
{
	public class BoneAnimation
	{
		public NursiaModelBone Bone { get; }
		public List<AnimationTransformKeyframe> Values { get; } = new List<AnimationTransformKeyframe>();

		public InterpolationMode TranslationInterpolation { get; set; }
		public InterpolationMode ScaleInterpolation { get; set; }
		public InterpolationMode RotationInterpolation { get; set; }

		public BoneAnimation(NursiaModelBone bone)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
		}

		/// <summary>
		/// Lower bound implementation taken from here: https://stackoverflow.com/a/39100135
		/// </summary>
		/// <param name="passed"></param>
		/// <returns></returns>
		public int FindIndexByTime(float passed)
		{
			if (Values.Count <= 1)
			{
				return 0;
			}

			var i = 0;
			for(; i < Values.Count; ++i)
			{
				if (passed < Values[i].Time)
				{
					break;
				}
			}

			return i;
		}
	}
}
