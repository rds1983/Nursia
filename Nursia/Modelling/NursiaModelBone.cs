using Microsoft.Xna.Framework;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Nursia.Modelling
{
	public class NursiaModelBone : ItemWithId
	{
		public ModelInstance Model { get; }
		public NursiaModelBone Parent { get; internal set; }
		public ObservableCollection<NursiaModelBone> Children { get; } = new ObservableCollection<NursiaModelBone>();

		public ObservableCollection<NursiaModelMesh> Meshes { get; } = new ObservableCollection<NursiaModelMesh>();

		public Skin Skin { get; set; }

		public Vector3 DefaultTranslation = Vector3.Zero;
		public Vector3 DefaultScale = Vector3.One;
		public Quaternion DefaultRotation = Quaternion.Identity;

		public Matrix Transform = Matrix.Identity;
		public Matrix InverseBindTransform = Matrix.Identity;

		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;
		public BoundingBox BoundingBox { get; internal set; }

		public bool HasSkin => Skin != null;

		public NursiaModelBone(ModelInstance model)
		{
			Model = model ?? throw new ArgumentNullException(nameof(model));

			Children.CollectionChanged += Children_CollectionChanged;
			Meshes.CollectionChanged += Meshes_CollectionChanged;
		}

		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (NursiaModelBone n in args.NewItems)
				{
					n.Parent = this;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (NursiaModelBone n in args.OldItems)
				{
					n.Parent = null;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var n in Children)
				{
					n.Parent = null;
				}
			}
		}

		private void Meshes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (NursiaModelMesh n in args.NewItems)
				{
					n.ParentBone = this;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (NursiaModelMesh n in args.OldItems)
				{
					n.ParentBone = null;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var n in Meshes)
				{
					n.ParentBone = null;
				}
			}
		}


		internal void Render(NursiaModel node, RenderBatch batch, ref Matrix rootTransform)
		{
			if (Meshes.Count > 0)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix meshTransform = HasSkin ? rootTransform : AbsoluteTransform * rootTransform;
				Matrix[] bonesTransforms = null;

				// Apply the effect and render items
				if (HasSkin)
				{
					bonesTransforms = Skin.CalculateBoneTransforms();
				}

				foreach (var mesh in Meshes)
				{
					var m = mesh.Transform * AbsoluteTransform * rootTransform;
					if (HasSkin)
					{
						mesh.Mesh.BonesTransforms = bonesTransforms;
					}

					batch.BatchJob(mesh.Material, mesh.Transform * meshTransform, mesh.Mesh);
				}
			}

			foreach (var child in Children)
			{
				child.Render(node, batch, ref rootTransform);
			}
		}
	}
}
