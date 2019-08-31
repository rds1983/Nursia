using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D
{
	public class Camera
	{
		private Vector3 _position;
		private float _yawAngle, _pitchAngle;
		private Vector3 _up, _right, _direction;
		private Matrix _view;
		private bool _dirty = true;

		public Vector3 Position
		{
			get { return _position; }
			set
			{
				if (_position != value)
				{
					_position = value;
					Invalidate();
				}
			}
		}

		public float YawAngle
		{
			get { return _yawAngle; }
			set
			{
				if (_yawAngle != value)
				{
					_yawAngle = value;
					Invalidate();
				}
			}
		}

		public float PitchAngle
		{
			get { return _pitchAngle; }
			set
			{
				if (_pitchAngle != value)
				{
					_pitchAngle = value;
					Invalidate();
				}
			}
		}

		public Vector3 Direction
		{
			get
			{
				Update();
				return _direction;
			}
		}

		public Vector3 Up
		{
			get
			{
				Update();
				return _up;
			}
		}

		public Vector3 Right
		{
			get
			{
				Update();
				return _right;
			}
		}

		public Matrix View
		{
			get
			{
				Update();
				return _view;
			}
		}

		public Camera()
		{
		}

		public void SetLookAt(Vector3 position,
			Vector3 target,
			Vector3 up)
		{
			Position = position;

			var direction = target - _position;
			direction.Normalize();

			PitchAngle = 360 - MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(direction, Vector3.Forward)));
			YawAngle = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(up, Vector3.Up)));
		}

		private void Invalidate()
		{
			_dirty = true;
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			var rotation = Matrix.CreateFromYawPitchRoll(
				MathHelper.ToRadians(YawAngle), 
				MathHelper.ToRadians(PitchAngle), 
				0);

			_direction = Vector3.Transform(new Vector3(0, 0, -1), rotation);
			_up = Vector3.Transform(Vector3.Up, rotation);
			_right = Vector3.Cross(_direction, _up);
			_right.Normalize();

			_view = Matrix.CreateLookAt(Position, Position + _direction, _up);

			_dirty = false;
		}
	}
}