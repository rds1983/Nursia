using Microsoft.Xna.Framework;
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

		public ModelNode RootNode { get; set; }

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

		public ModelNode FindNodeById(string id)
		{
			return RootNode.FindNodeById(id);
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
			TraverseNodes(RootNode, action);
		}

		internal void UpdateNodesAbsoluteTransforms()
		{
			if (RootNode == null)
			{
				return;
			}

			RootNode.UpdateAbsoluteTransforms(Matrix.Identity);
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

			TimeSpan? passed = null;
			var now = DateTime.Now;
			if (_lastAnimationUpdate != null)
			{
				passed = now - _lastAnimationUpdate.Value;
			}

			var allFound = true;
			foreach (var bone in _currentAnimation.BoneAnimations)
			{
				if (bone.Frames.Count == 0)
				{
					continue;
				}

				if (passed == null)
				{
					// Use first frame
					bone.Node.Transform = bone.Frames[0].Transform;
					continue;
				}

				var found = false;

				foreach (var frame in bone.Frames)
				{
					if (bone.Frames.Count == 1 ||
						passed < frame.Time)
					{
						// Use this frame
						found = true;
						bone.Node.Transform = frame.Transform;
						break;
					}
				}

				if (!found)
				{
					allFound = false;
				}
			}

			if (_lastAnimationUpdate == null || !allFound)
			{
				_lastAnimationUpdate = now;
			}
		}
	}
}