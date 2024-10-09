using Microsoft.Xna.Framework;
using Nursia.Modelling;
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

		public void UpdateCurrentAnimation(float passed)
		{
			if (_currentAnimation == null)
			{
				return;
			}

			foreach (var boneAnimation in _currentAnimation.BoneAnimations)
			{
				var bone = boneAnimation.Bone;

				Pose pose;
				var i = boneAnimation.FindIndexByTime(passed);

				if (i >= boneAnimation.Values.Count)
				{
					continue;
				}

				if (i == 0)
				{
					pose = boneAnimation.Values[0].Pose;
				}
				else
				{
					pose = Pose.Interpolate(boneAnimation.Values[i - 1].Pose,
						boneAnimation.Values[i].Pose,
						MathHelper.Clamp(passed - boneAnimation.Values[i - 1].Time, 0, 1),
						boneAnimation.TranslationInterpolation,
						boneAnimation.RotationInterpolation,
						boneAnimation.ScaleInterpolation);
				}

				_node.SetLocalTransform(bone.Index, pose.ToMatrix());
			}
		}
	}
}
