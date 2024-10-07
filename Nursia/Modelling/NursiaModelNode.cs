using AssetManagementBase;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Modelling
{
	[EditorInfo("Gltf/Glb Model")]
	public class NursiaModelNode : SceneNode
	{
		private ModelAnimation _currentAnimation = null;
		private NursiaModelBone[] _traverseOrder;
		private Matrix[] _localTransforms;
		private Matrix[] _worldTransforms;
		private Matrix[] _skinTransforms;

		private NursiaModel _model;

		[Browsable(false)]
		[JsonIgnore]
		public NursiaModel Model
		{
			get => _model;

			set
			{
				if (value == _model)
				{
					return;
				}

				_model = value;

				_traverseOrder = null;
				_localTransforms = null;
				_worldTransforms = null;
				_skinTransforms = null;
				if (_model != null)
				{
					// Build correct traverse order starting from root
					var traverseOrder = new List<NursiaModelBone>();
					_model.TraverseNodes(n =>
					{
						traverseOrder.Add(n);
					});


					_traverseOrder = traverseOrder.ToArray();
					_localTransforms = new Matrix[_model.Bones.Length];
					_worldTransforms = new Matrix[_model.Bones.Length];

					if (_model.Skin != null)
					{
						_skinTransforms = new Matrix[_model.Skin.Joints.Length];
					}

					ResetTransforms();
				}
			}
		}

		[Browsable(false)]
		public string ModelPath { get; set; }

		[Browsable(false)]
		[JsonIgnore]
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

		public void ResetTransforms()
		{
			if (Model == null)
			{
				return;
			}

			for (var i = 0; i < _traverseOrder.Length; i++)
			{
				var bone = _traverseOrder[i];
				_localTransforms[bone.Index] = Mathematics.CreateTransform(bone.DefaultTranslation, bone.DefaultScale, bone.DefaultRotation);
			}
		}

		private void UpdateWorldTransforms()
		{
			for (var i = 0; i < _traverseOrder.Length; i++)
			{
				var bone = _traverseOrder[i];

				if (bone.Parent == null)
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index];
				}
				else
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index] * _worldTransforms[bone.Parent.Index];
				}
			}
		}

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			if (Model == null)
			{
				return;
			}

			UpdateWorldTransforms();

			// Render meshes
			var rootTransform = GlobalTransform;
			foreach (var mesh in _model.Meshes)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix transform = _model.Skin != null ? rootTransform : _worldTransforms[mesh.ParentBone.Index] * rootTransform;

				// Apply the effect and render items
				if (_model.Skin != null)
				{
					for (var i = 0; i < _model.Skin.Joints.Length; ++i)
					{
						var joint = _model.Skin.Joints[i];

						_skinTransforms[i] = joint.InverseBindTransform * _worldTransforms[joint.Bone.Index];
					}

					mesh.Mesh.BonesTransforms = _skinTransforms;
				}

				batch.BatchJob(mesh.Material, transform, mesh.Mesh);
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Model = assetManager.LoadGltf(ModelPath);
		}

		public BoundingBox CalculateBoundingBox()
		{
			UpdateWorldTransforms();

			var boundingBox = new BoundingBox();
			foreach (var mesh in _model.Meshes)
			{
				var m = _worldTransforms[mesh.ParentBone.Index];
				var bb = mesh.BoundingBox.Transform(ref m);
				boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
			}

			return boundingBox;
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

				_localTransforms[bone.Index] = Mathematics.CreateTransform(translation, scale, rotation);
			}
		}
	}
}
