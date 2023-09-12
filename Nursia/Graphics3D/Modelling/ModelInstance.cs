using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using static glTFLoader.Schema.AnimationSampler;

namespace Nursia.Graphics3D.Modelling
{
	public class ModelInstance
	{
		private ModelAnimation _currentAnimation = null;

		public Matrix Transform = Matrix.Identity;

		public NursiaModel Model { get; } = new NursiaModel();

		public List<NodeInstance> AllNodes { get; } = new List<NodeInstance>();
		public List<NodeInstance> RootNodes { get; } = new List<NodeInstance>();


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

				if (value != null && !Model.Animations.ContainsValue(value))
				{
					throw new ArgumentException("This animation doesnt not belong to this model");
				}

				_currentAnimation = value;
				ResetTransforms();
			}
		}

		public BoundingBox BoundingBox { get; internal set; }

		internal ModelInstance(NursiaModel model)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));

			foreach(var node in model.AllNodes)
			{
				AllNodes.Add(new NodeInstance(this, node));
			}

			foreach(var root in model.RootNodes)
			{
				var instance = (from n in AllNodes where n.Node == root select n).First();
				RootNodes.Add(instance);
			}
		}

		private void TraverseNodes(NodeInstance root, Action<NodeInstance> action)
		{
			action(root);

			foreach(var index in root.Node.ChildrenIndices)
			{
				TraverseNodes(AllNodes[index], action);
			}
		}

		internal void TraverseNodes(Action<NodeInstance> action)
		{
			foreach (var node in RootNodes)
			{
				TraverseNodes(node, action);
			}
		}

		internal void UpdateNodesAbsoluteTransforms()
		{
			TraverseNodes(n =>
			{
				if (n.Node.ParentIndex == null)
				{
					n.AbsoluteTransform = n.Transform;
				}
				else
				{
					n.AbsoluteTransform = n.Transform * AllNodes[n.Node.ParentIndex.Value].AbsoluteTransform;
				}
			});
		}

		public void ResetTransforms()
		{
			TraverseNodes(n => {
				n.Transform = Mathematics.CreateTransform(n.Node.DefaultTranslation, n.Node.DefaultScale, n.Node.DefaultRotation);
			});
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
				var node = AllNodes[boneAnimation.NodeIndex];

				var translation = GetAnimationTransform(boneAnimation.Translations, passed, node.Node.DefaultTranslation);
				var scale = GetAnimationTransform(boneAnimation.Scales, passed, node.Node.DefaultScale);
				var rotation = GetAnimationTransform(boneAnimation.Rotations, passed, node.Node.DefaultRotation);

				node.Transform = Mathematics.CreateTransform(translation, scale, rotation);
			}
		}
	}
}
