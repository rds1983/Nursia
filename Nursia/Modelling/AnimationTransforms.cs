using System.Collections.Generic;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;

namespace Nursia.Modelling
{
	public abstract class AnimationTransforms<T>
	{
		public List<AnimationTransformKeyframe<T>> Values { get; } = new List<AnimationTransformKeyframe<T>>();

		public InterpolationEnum Interpolation { get; set; }

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

			if (Values.Count == 2)
			{
				return 1;
			}

			if (passed >= Values[Values.Count - 1].Time)
			{
				// Beyond last frame
				return Values.Count - 1;
			}

			int start = 0;
			int end = Values.Count;

			while (start < end)
			{
				var middle = start + ((end - start) >> 1);
				if (passed < Values[middle].Time)
				{
					end = middle;
				}
				else
				{
					start = middle + 1;
				}
			}

			return start;
		}

		public abstract T CalculateInterpolatedValue(float passed, int frameIndex);
	}

	internal class AnimationTransformsVector3 : AnimationTransforms<Vector3>
	{
		public override Vector3 CalculateInterpolatedValue(float passed, int frameIndex)
		{
			var k = Values[frameIndex].DeltaK * (passed - Values[frameIndex - 1].Time);

			return (Values[frameIndex - 1].Value * (1 - k)) + (Values[frameIndex].Value * k);
		}
	}

	internal class AnimationTransformsQuaternion : AnimationTransforms<Quaternion>
	{
		public override Quaternion CalculateInterpolatedValue(float passed, int frameIndex)
		{
			var k = Values[frameIndex].DeltaK * (passed - Values[frameIndex - 1].Time);

			var result = Quaternion.Slerp(Values[frameIndex - 1].Value, Values[frameIndex].Value, k);

			return result;
		}
	}
}
