﻿using Microsoft.Xna.Framework;
using Nursia.Utilities;

namespace Nursia.Modelling
{
	public class NursiaModelBone
	{
		private NursiaModelBone[] _children;
		private NursiaModelMesh[] _meshes;

		public int Index { get; }
		public string Name { get; }
		public NursiaModelBone Parent { get; private set; }

		public NursiaModelBone[] Children
		{
			get => _children;

			internal set
			{
				if (value != null)
				{
					foreach (var b in value)
					{
						b.Parent = this;
					}
				}

				_children = value;
			}

		}

		public NursiaModelMesh[] Meshes
		{
			get => _meshes;

			internal set
			{
				if (value != null)
				{
					foreach (var m in value)
					{
						m.ParentBone = this;
					}
				}

				_meshes = value;
			}
		}


		public Vector3 DefaultTranslation = Vector3.Zero;
		public Vector3 DefaultScale = Vector3.One;
		public Quaternion DefaultRotation = Quaternion.Identity;

		internal NursiaModelBone(int index, string name)
		{
			Index = index;
			Name = name;
		}
		public override string ToString() => Name;

		public Matrix CalculateDefaultLocalTransform() => Mathematics.CreateTransform(DefaultTranslation, DefaultScale, DefaultRotation);
		public Matrix CalculateDefaultAbsoluteTransform()
		{
			if (Parent == null)
			{
				return CalculateDefaultLocalTransform();
			}

			return CalculateDefaultLocalTransform() * Parent.CalculateDefaultAbsoluteTransform();
		}
	}
}
