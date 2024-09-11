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
	public enum NodeBlendMode
	{
		Opaque,
		Transparent
	}

	/// <summary>
	/// Base 3D Scene Node
	/// </summary>
	public class SceneNode : ItemWithId
	{
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Matrix? _globalTransform = null, _localTransform = null;

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
				InvalidateTransform();
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
				InvalidateTransform();
			}
		}

		public Vector3 Rotation
		{
			get => _rotation;

			set
			{
				if (value == _rotation)
				{
					return;
				}

				_rotation = value;
				InvalidateTransform();
			}
		}

		public NodeBlendMode BlendMode { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public Matrix LocalTransform
		{
			get
			{
				if (_localTransform == null)
				{
					var quaternion = Quaternion.CreateFromYawPitchRoll(
						MathHelper.ToRadians(_rotation.Y),
						MathHelper.ToRadians(_rotation.X),
						MathHelper.ToRadians(_rotation.Z));
					_localTransform = Mathematics.CreateTransform(Translation, Scale, quaternion);
				}

				return _localTransform.Value;
			}
		}


		[Browsable(false)]
		[JsonIgnore]
		public Matrix GlobalTransform
		{
			get
			{
				if (_globalTransform == null)
				{
					if (Parent != null)
					{
						_globalTransform = LocalTransform * Parent.GlobalTransform;
					}
					else
					{
						_globalTransform = LocalTransform;
					}
				}

				return _globalTransform.Value;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent { get; internal set; }

		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		[Browsable(false)]
		[JsonIgnore]
		public object Tag { get; set; }

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

		public void InvalidateTransform()
		{
			_localTransform = null;
			_globalTransform = null;

			foreach (var child in Children)
			{
				child.InvalidateTransform();
			}
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
