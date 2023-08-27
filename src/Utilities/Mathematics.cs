using System;
using Microsoft.Xna.Framework;

namespace Nursia.Utilities
{
	internal static class Mathematics
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		/// <summary>
		/// Compares two floating point numbers based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="left">The first number to compare.</param>
		/// <param name="right">The second number to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is within epsilon of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this float left, float right, float epsilon = ZeroTolerance)
		{
			return Math.Abs(left - right) <= epsilon;
		}

		public static bool IsZero(this float a)
		{
			return a.EpsilonEquals(0.0f);
		}

		public static Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix viewProjection, bool clipSide)
		{
			planeNormalDirection.Normalize();
			Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
			if (clipSide)
				planeCoeffs *= -1;

			Matrix inverseWorldViewProjection = Matrix.Invert(viewProjection);
			inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

			planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
			Plane finalPlane = new Plane(planeCoeffs);

			return finalPlane;
		}

		public static Vector3 ToVector3(this float[] array) => new Vector3(array[0], array[1], array[2]);
		public static Vector4 ToVector4(this float[] array) => new Vector4(array[0], array[1], array[2], array[3]);
		public static Quaternion ToQuaternion(this float[] array) => new Quaternion(array[0], array[1], array[2], array[3]);

		public static Matrix CreateTransform(Vector3 translation, Vector3 scale, Quaternion rotation)
		{
			return Matrix.CreateFromQuaternion(rotation) *
				Matrix.CreateScale(scale) *
				Matrix.CreateTranslation(translation);
		}

		public static BoundingBox Transform(this BoundingBox source, ref Matrix matrix)
		{
			// Transform corners
			var corners = source.GetCorners();

			var min = new Vector3(float.MaxValue);
			var max = new Vector3(float.MinValue);
			for (var i = 0; i < corners.Length; ++i)
			{
				var c = corners[i];
				Vector3.Transform(ref c, ref matrix, out Vector3 v);

				if (v.X < min.X)
				{
					min.X = v.X;
				}

				if (v.Y < min.Y)
				{
					min.Y = v.Y;
				}

				if (v.Z < min.Z)
				{
					min.Z = v.Z;
				}

				if (v.X > max.X)
				{
					max.X = v.X;
				}

				if (v.Y > max.Y)
				{
					max.Y = v.Y;
				}

				if (v.Z > max.Z)
				{
					max.Z = v.Z;
				}
			}

			return new BoundingBox(min, max);
		}
	}
}
