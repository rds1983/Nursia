using Microsoft.Xna.Framework;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public partial class NursiaModel
	{
		private ModelAnimation _currentAnimation = null;
		private DateTime? _lastAnimationUpdate;

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
				_lastAnimationUpdate = null;
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

		internal void ResetTransforms()
		{
			TraverseNodes(n => n.Transform = n.DefaultTransform);
		}

		public void UpdateCurrentAnimation()
		{
			if (_currentAnimation == null)
			{
				return;
			}

			var now = DateTime.Now;
			if (_lastAnimationUpdate == null)
			{
				_lastAnimationUpdate = now;
				return;
			}

			var allFound = true;
			var passed = now - _lastAnimationUpdate.Value;

			foreach (var boneAnimation in _currentAnimation.BoneAnimations)
			{
				if (boneAnimation.Frames.Count == 0)
				{
					continue;
				}

				var found = false;

				var translation = Vector3.Zero;
				var rotation = Quaternion.Identity;
				var scale = Vector3.One;

				foreach (var frame in boneAnimation.Frames)
				{
					if (frame.Translation != null)
					{
						translation = frame.Translation.Value;
					}

					if (frame.Rotation != null)
					{
						rotation = frame.Rotation.Value;
					}

					if (frame.Scale != null)
					{
						scale = frame.Scale.Value;
					}

					if (boneAnimation.Frames.Count == 1 || passed < frame.Time)
					{
						// Use this frame
						found = true;

						boneAnimation.Node.Transform = Mathematics.CreateTransform(translation, scale, rotation);
						break;
					}
				}

				if (!found)
				{
					allFound = false;
				}
			}

			if (!allFound)
			{
				_lastAnimationUpdate = now;
			}
		}
	}
}