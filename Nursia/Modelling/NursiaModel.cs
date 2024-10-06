using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Modelling
{
	public class NursiaModel
	{
		private ModelAnimation _currentAnimation = null;

		public NursiaModelBone[] Bones { get; }
		public NursiaModelMesh[] Meshes { get; }
		public NursiaModelBone Root { get; }

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

		public BoundingBox BoundingBox { get; internal set; }

		internal NursiaModel(NursiaModelBone[] bones, NursiaModelMesh[] meshes, int rootIndex = 0)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no bones");
			}

			if (meshes == null)
			{
				throw new ArgumentNullException(nameof(meshes));
			}

			if (meshes.Length == 0)
			{
				throw new ArgumentException(nameof(meshes), "no meshes");
			}

			if (rootIndex < 0 || rootIndex >= bones.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(rootIndex));
			}

			Bones = bones;
			Meshes = meshes;
			Root = bones[rootIndex];
		}

		private void TraverseNodes(NursiaModelBone root, Action<NursiaModelBone> action)
		{
			action(root);

			foreach (var child in root.Children)
			{
				TraverseNodes(child, action);
			}
		}

		internal void TraverseNodes(Action<NursiaModelBone> action)
		{
			TraverseNodes(Root, action);
		}

		internal void UpdateNodesAbsoluteTransforms()
		{
			TraverseNodes(n =>
			{
				if (n.Parent == null)
				{
					n.AbsoluteTransform = n.Transform;
				}
				else
				{
					n.AbsoluteTransform = n.Transform * n.Parent.AbsoluteTransform;
				}
			});
		}

		public void ResetTransforms()
		{
			TraverseNodes(n =>
			{
				n.Transform = Mathematics.CreateTransform(n.DefaultTranslation, n.DefaultScale, n.DefaultRotation);
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
				var node = boneAnimation.Node;

				var translation = GetAnimationTransform(boneAnimation.Translations, passed, node.DefaultTranslation);
				var scale = GetAnimationTransform(boneAnimation.Scales, passed, node.DefaultScale);
				var rotation = GetAnimationTransform(boneAnimation.Rotations, passed, node.DefaultRotation);

				node.Transform = Mathematics.CreateTransform(translation, scale, rotation);
			}
		}

		public void UpdateBoundingBox()
		{
			ResetTransforms();
			UpdateNodesAbsoluteTransforms();

			var boundingBox = new BoundingBox();
			TraverseNodes(n =>
			{
				var m = n.AbsoluteTransform;
				foreach (var mesh in n.Meshes)
				{
					var bb = mesh.BoundingBox.Transform(ref m);
					boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
				}
			});

			BoundingBox = boundingBox;
		}

		public void Render(RenderBatch batch, ref Matrix transform)
		{
			UpdateNodesAbsoluteTransforms();

			Root.Render(batch, ref transform);
		}
	}
}