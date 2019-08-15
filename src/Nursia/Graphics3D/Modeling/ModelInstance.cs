using Microsoft.Xna.Framework;
using Nursia.Graphics3D.Materials;
using System;

namespace Nursia.Graphics3D.Modeling
{
	public class ModelInstance: ItemWithId
	{
		private readonly Sprite3D _model;

		public Sprite3D Model
		{
			get
			{
				return _model;
			}
		}

		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }

		public Vector3 Scale { get; set; }
		public Vector3 Translate { get; set; }

		public Matrix Transform
		{
			get
			{
				return Matrix.CreateRotationY(MathHelper.ToRadians(RotationX)) *
					   Matrix.CreateRotationX(MathHelper.ToRadians(RotationY)) *
					   Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ)) *
					   Matrix.CreateScale(Scale) *
					   Matrix.CreateTranslation(Translate);
			}
		}

		public BaseMaterial Material { get; set; }

		public ModelInstance(Sprite3D model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			_model = model;
			Scale = new Vector3(1, 1, 1);
		}
	}
}
