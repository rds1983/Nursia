using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nursia.Rendering
{
	/// <summary>
	/// Base 3D Scene Node
	/// </summary>
	public class SceneNode : ItemWithId
	{
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Quaternion _rotation = Quaternion.Identity;
		private Matrix? _transform = null;

		public Vector3 Translation
		{
			get => _translation;

			set
			{
				if (value == _translation)
				{
					return;
				}

				_translation = value;
				_transform = null;
			}
		}

		public Vector3 Scale
		{
			get => _scale;

			set
			{
				if (value == _scale)
				{
					return;
				}

				_scale = value;
				_transform = null;
			}
		}

		public Quaternion Rotation
		{
			get => _rotation;

			set
			{
				if (value == _rotation)
				{
					return;
				}

				_rotation = value;
				_transform = null;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public Matrix Transform
		{
			get
			{
				if (_transform == null)
				{
					_transform = Mathematics.CreateTransform(Translation, Scale, Rotation);
				}

				return _transform.Value;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent { get; internal set; }

		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		public SceneNode()
		{
			Children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		public virtual void Load(AssetManager assetManager)
		{
			foreach (var child in Children)
			{
				child.Load(assetManager);
			}
		}

		private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (SceneNode n in args.NewItems)
				{
					OnChildAdded(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (SceneNode n in args.OldItems)
				{
					OnChildRemoved(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var w in Children)
				{
					OnChildRemoved(w);
				}
			}
		}

		protected virtual void OnChildAdded(SceneNode n)
		{
			n.Parent = this;
		}

		protected virtual void OnChildRemoved(SceneNode n)
		{
			n.Parent = null;
		}

		protected internal virtual void Render(RenderContext context)
		{
		}

		private void InternalQuery(Func<SceneNode, bool> predicate, List<SceneNode> result)
		{
			if (predicate(this))
			{
				result.Add(this);
			}

			foreach (var child in Children)
			{
				child.InternalQuery(predicate, result);
			}
		}

		public List<SceneNode> Query(Func<SceneNode, bool> predicate)
		{
			var result = new List<SceneNode>();
			InternalQuery(predicate, result);

			return result;
		}

		private void InternalQueryByType<T>(List<T> result) where T : SceneNode
		{
			var asT = this as T;
			if (asT != null)
			{
				result.Add(asT);
			}

			foreach (var child in Children)
			{
				child.InternalQueryByType(result);
			}
		}

		public List<T> QueryByType<T>() where T : SceneNode
		{
			var result = new List<T>();

			InternalQueryByType<T>(result);

			return result;
		}

		public void Iterate(Action<SceneNode> action)
		{
			action(this);

			foreach (var child in Children)
			{
				action(child);
			}
		}

		public void RemoveFromParent()
		{
			if (Parent == null)
			{
				return;
			}

			Parent.Children.Remove(this);
		}
	}
}
