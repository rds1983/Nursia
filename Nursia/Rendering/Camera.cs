using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Utilities;
using System;

namespace Nursia.Rendering
{
	public class Camera
	{
		private Vector3 _position;
		private float _yawAngle, _pitchAngle, _rollAngle;
		private float _viewAngle = 90.0f;
		private Vector3 _up, _right, _direction;
		private Matrix _view;
		private float _nearPlaneDistance = 0.1f;
		private float _farPlaneDistance = 1000f;

		private bool _dirty = true;
		private string _string;

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

		/// <summary>
		/// Yaw Angle in Degrees
		/// </summary>
		public float YawAngle
		{
			get { return _yawAngle; }
			set
			{
				value = ClampDegree(value);
				if (_yawAngle != value)
				{
					_yawAngle = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Pitch Angle In Degrees
		/// </summary>
		public float PitchAngle
		{
			get { return _pitchAngle; }
			set
			{
				value = ClampDegree(value);
				if (_pitchAngle != value)
				{
					_pitchAngle = value;
					Invalidate();
				}
			}
		}

		public float RollAngle
		{
			get { return _rollAngle; }
			set
			{
				value = ClampDegree(value);
				if (_rollAngle != value)
				{
					_rollAngle = value;
					Invalidate();
				}
			}
		}

		public float ViewAngle
		{
			get
			{
				return _viewAngle;
			}

			set
			{
				_viewAngle = value;
			}
		}

		public float NearPlaneDistance
		{
			get => _nearPlaneDistance;

			set
			{
				if (value.EpsilonEquals(_nearPlaneDistance))
				{
					return;
				}

				_nearPlaneDistance = value;
			}
		}

		public float FarPlaneDistance
		{
			get => _farPlaneDistance;

			set
			{
				if (value.EpsilonEquals(_farPlaneDistance))
				{
					return;
				}

				_farPlaneDistance = value;
			}
		}

		[JsonIgnore]
		public Vector3 Direction
		{
			get
			{
				Update();
				return _direction;
			}
		}

		[JsonIgnore]
		public Vector3 Target => Position + Direction;


		[JsonIgnore]
		public Vector3 Up
		{
			get
			{
				Update();
				return _up;
			}
		}

		[JsonIgnore]
		public Vector3 Right
		{
			get
			{
				Update();
				return _right;
			}
		}

		[JsonIgnore]
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

		public void SetLookAt(Vector3 position, Vector3 target)
		{
			Position = position;

			var direction = target - _position;
			direction.Normalize();

			PitchAngle = 360 - MathHelper.ToDegrees((float)Math.Asin(direction.Y));
			YawAngle = MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));
		}

		private float ClampDegree(float deg)
		{
			var isNegative = deg < 0;
			deg = Math.Abs(deg);
			deg = deg % 360;
			if (isNegative)
			{
				deg = 360 - deg;
			}

			return deg;
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
				MathHelper.ToRadians(RollAngle));

			_direction = Vector3.Transform(Vector3.Backward, rotation);
			_up = Vector3.Transform(Vector3.Up, rotation);
			_right = Vector3.Cross(_direction, _up);
			_right.Normalize();

			_view = Matrix.CreateLookAt(Position, Position + _direction, _up);

			_string = string.Format("{0:0.##}/{1:0.##}/{2:0.##};{3:0.##};{4:0.##}",
				Position.X,
				Position.Y,
				Position.Z,
				YawAngle,
				PitchAngle);

			_dirty = false;
		}

		public Matrix CalculateProjection(float aspectRatio) =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				aspectRatio, NearPlaneDistance, FarPlaneDistance);

		public Matrix CalculateProjection() => CalculateProjection(Nrs.GraphicsDevice.Viewport.AspectRatio);

		public override string ToString()
		{
			Update();
			return _string;
		}

		public Camera Clone()
		{
			return new Camera
			{
				Position = Position,
				YawAngle = YawAngle,
				PitchAngle = PitchAngle,
				RollAngle = RollAngle,
				ViewAngle = ViewAngle,
				NearPlaneDistance = NearPlaneDistance,
				FarPlaneDistance = FarPlaneDistance
			};
		}
	}
}