using System.Collections.Generic;
using Microsoft.Xna.Framework;
using static glTFLoader.Schema.AnimationSampler;

namespace Nursia.Graphics3D.Modelling
{
	public abstract class AnimationTransforms<T>
	{
		public List<AnimationTransformKeyframe<T>> Values { get; } = new List<AnimationTransformKeyframe<T>>();
		public InterpolationEnum Interpolation { get; set; }

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
