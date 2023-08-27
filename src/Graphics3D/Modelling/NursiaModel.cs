using Microsoft.Xna.Framework;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using static glTFLoader.Schema.AnimationSampler;

namespace Nursia.Graphics3D.Modelling
{
	public partial class NursiaModel
	{
		private ModelAnimation _currentAnimation = null;

		public Matrix Transform = Matrix.Identity;

		public List<ModelNode> Meshes { get; } = new List<ModelNode>();

		public List<Material> Materials { get; } = new List<Material>();

		public Dictionary<string, ModelAnimation> Animations { get; } = new Dictionary<string, ModelAnimation>();

		public ModelAnimation CurrentAnimation
		{
			get
			{
				return _currentAnimation;
			}

			set
			{
				if (value == _currentAnimation)
				{
					return;
				}

				if (value != null && !Animations.ContainsValue(value))
				{
					throw new ArgumentException("This animation doesnt not belong to this model");
				}

				_currentAnimation = value;
				ResetTransforms();
			}
		}

		private static void TraverseNodes(ModelNode root, Action<ModelNode> action)
		{
			if (root == null)
			{
				return;
			}

			action(root);

			foreach (var child in root.Children)
			{
				TraverseNodes(child, action);
			}
		}

		internal void TraverseNodes(Action<ModelNode> action)
		{
			foreach (var node in Meshes)
			{
				TraverseNodes(node, action);
			}
		}

		internal void UpdateNodesAbsoluteTransforms()
		{
			foreach (var child in Meshes)
			{
				child.UpdateAbsoluteTransforms(Matrix.Identity);
			}
		}

		public void ResetTransforms()
		{
			TraverseNodes(n => n.Transform = Mathematics.CreateTransform(n.DefaultTranslation, n.DefaultScale, n.DefaultRotation));
		}

		private static T GetAnimationTransform<T>(AnimationTransforms<T> transformFrames, float passed, T defaultValue)
		{
			if (transformFrames.Values.Count == 0)
			{
				return defaultValue;
			}

			var lastFrame = transformFrames.Values[transformFrames.Values.Count - 1];
			if (passed >= lastFrame.Time)
			{
				return lastFrame.Value;
			}

			var result = defaultValue;
			for (var i = 0; i < transformFrames.Values.Count; i++)
			{
				var frame = transformFrames.Values[i];
				if (passed < frame.Time)
				{
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

					break;
				}
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
				var node = boneAnimation.Node;

				var translation = GetAnimationTransform(boneAnimation.Translations, passed, node.DefaultTranslation);
				var scale = GetAnimationTransform(boneAnimation.Scales, passed, node.DefaultScale);
				var rotation = GetAnimationTransform(boneAnimation.Rotations, passed, node.DefaultRotation);

				boneAnimation.Node.Transform = Mathematics.CreateTransform(translation, scale, rotation);
			}
		}
	}
}