using glTFLoader.Schema;
using Nursia.Modelling;
using Nursia.Utilities;
using System;

namespace Nursia.Animation
{
	public class AnimationPlayer
	{
		private AnimationClip _currentAnimation = null;

		private readonly NursiaModelNode _node;

		public AnimationClip CurrentAnimation => _currentAnimation;

		public AnimationPlayer(NursiaModelNode node)
		{
			_node = node ?? throw new ArgumentNullException(nameof(node));
		}

		public void StartAnimation(string name)
		{
			AnimationClip animation;
			if (!_node.Model.Animations.TryGetValue(name, out animation))
			{
				throw new Exception($"Could not find animation '{name}'");
			}

			_currentAnimation = animation;
			_node.ResetTransforms();
		}

		private static T GetAnimationTransform<T>(AnimationTransforms<T> transformFrames, float passed, T defaultValue)
		{
			if (transformFrames.Values.Count == 0)
			{
				return defaultValue;
			}

			var i = transformFrames.FindIndexByTime(passed);
			T result;
			if (i > 0)
			{
				if (transformFrames.Interpolation == InterpolationEnum.STEP)
				{
					result = transformFrames.Values[i - 1].Value;
				}
				else
				{
					result = transformFrames.CalculateInterpolatedValue(passed, i);
				}
			}
			else
			{
				result = transformFrames.Values[i].Value;
			}

			return result;
		}

		public void UpdateCurrentAnimation(float passed)
		{
			if (_currentAnimation == null)
			{
				return;
			}

			foreach (var boneAnimation in _currentAnimation.BoneAnimations)
			{
				var bone = boneAnimation.Bone;

				var translation = GetAnimationTransform(boneAnimation.Translations, passed, bone.DefaultTranslation);
				var scale = GetAnimationTransform(boneAnimation.Scales, passed, bone.DefaultScale);
				var rotation = GetAnimationTransform(boneAnimation.Rotations, passed, bone.DefaultRotation);

				_node.SetLocalTransform(bone.Index, Mathematics.CreateTransform(translation, scale, rotation));
			}
		}
	}
}
