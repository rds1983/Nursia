using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Modelling
{
	[EditorInfo("Gltf/Glb Model")]
	public class NursiaModelNode : SceneNode
	{
		private bool _transformsDirty = true;
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

			_transformsDirty = true;
		}

		private void UpdateTransforms()
		{
			if (!_transformsDirty)
			{
				return;
			}

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

			// Update skin transforms
			if (_model.Skin != null)
			{
				for (var i = 0; i < _model.Skin.Joints.Length; ++i)
				{
					var joint = _model.Skin.Joints[i];

					_skinTransforms[i] = joint.InverseBindTransform * _worldTransforms[joint.Bone.Index];
				}
			}

			_transformsDirty = false;
		}

		protected internal override void Render(RenderBatch batch)
		{
			base.Render(batch);

			if (Model == null)
			{
				return;
			}

			UpdateTransforms();

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
			UpdateTransforms();

			var boundingBox = new BoundingBox();
			foreach (var mesh in _model.Meshes)
			{
				var m = _worldTransforms[mesh.ParentBone.Index];
				var bb = mesh.BoundingBox.Transform(ref m);
				boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
			}

			return boundingBox;
		}

		public Matrix GetLocalTransform(int boneIndex) => _localTransforms[boneIndex];

		public void SetLocalTransform(int boneIndex, Matrix transform)
		{
			_localTransforms[boneIndex] = transform;
			_transformsDirty = true;
		}
	}
}
