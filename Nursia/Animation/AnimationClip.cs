using System.Collections.Generic;

namespace Nursia.Animation
{
	public class AnimationClip : ItemWithId
	{
		public List<BoneAnimation> BoneAnimations { get; } = new List<BoneAnimation>();
		public float Time { get; set; }

		private void UpdateStartEndInternal<T>(AnimationTransforms<T> transformFrames, ref float? start, ref float? end)
		{
			foreach (var frame in transformFrames.Values)
			{
				if (start == null || frame.Time < start.Value)
				{
					start = frame.Time;
				}

				if (end == null || frame.Time > end.Value)
				{
					end = frame.Time;
				}
			}
		}

		private void SubstractTime<T>(AnimationTransforms<T> transformFrames, float time)
		{
			foreach (var frame in transformFrames.Values)
			{
				frame.Time -= time;
			}
		}

		private void CalculateDeltas<T>(AnimationTransforms<T> transformFrames)
		{
			for (var i = 1; i < transformFrames.Values.Count; i++)
			{
				transformFrames.Values[i].DeltaK = 1.0f / (transformFrames.Values[i].Time - transformFrames.Values[i - 1].Time);
			}
		}

		public void UpdateStartEnd()
		{
			float? start = null, end = null;
			foreach (var animation in BoneAnimations)
			{
				UpdateStartEndInternal(animation.Translations, ref start, ref end);
				UpdateStartEndInternal(animation.Scales, ref start, ref end);
				UpdateStartEndInternal(animation.Rotations, ref start, ref end);
			}

			if (start == null || end == null)
			{
				return;
			}

			// Substract all frames by start
			foreach (var animation in BoneAnimations)
			{
				SubstractTime(animation.Translations, start.Value);
				SubstractTime(animation.Scales, start.Value);
				SubstractTime(animation.Rotations, start.Value);
			}

			// Calculate DeltaK
			foreach (var animation in BoneAnimations)
			{
				CalculateDeltas(animation.Translations);
				CalculateDeltas(animation.Scales);
				CalculateDeltas(animation.Rotations);
			}

			Time = end.Value - start.Value;
		}
	}
}
