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

		public static bool EpsilonEquals(this Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon);
		}

		public static bool EpsilonEquals(this Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon);
		}

		public static bool EpsilonEquals(this Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		public static bool IsZero(this float a)
		{
			return a.EpsilonEquals(0.0f);
		}

		public static bool IsZero(this Vector2 a)
		{
			return a.EpsilonEquals(Vector2.Zero);
		}

		public static bool IsZero(this Vector3 a)
		{
			return a.EpsilonEquals(Vector3.Zero);
		}

		public static bool IsZero(this Vector4 a)
		{
			return a.EpsilonEquals(Vector4.Zero);
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

		public static Matrix ToMatrix(this float[] array) =>
			new Matrix(array[0], array[1], array[2], array[3],
				array[4], array[5], array[6], array[7],
				array[8], array[9], array[10], array[11],
				array[12], array[13], array[14], array[15]);

		public static Matrix CreateTransform(Vector3 translation, Vector3 scale, Quaternion rotation)
		{
			return Matrix.CreateScale(scale) * 
				Matrix.CreateFromQuaternion(rotation) *
				Matrix.CreateTranslation(translation);
		}

		public static BoundingBox Transform(this BoundingBox source, ref Matrix matrix)
		{
			Vector3.Transform(ref source.Min, ref matrix, out Vector3 v1);
			Vector3.Transform(ref source.Max, ref matrix, out Vector3 v2);

			var min = new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
			var max = new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));

			return new BoundingBox(min, max);
		}

		public static Vector3 CalculateCenter(this BoundingBox source)
		{
			return new Vector3(
				source.Min.X + (source.Max.X - source.Min.X) / 2,
				source.Min.Y + (source.Max.Y - source.Min.Y) / 2,
				source.Min.Z + (source.Max.Z - source.Min.Z) / 2);
		}

		public static bool IsEmpty(this BoundingBox source)
		{
			var d = source.Max - source.Min;

			return d.X.IsZero() && d.Y.IsZero() && d.Z.IsZero();
		}

		public static Vector4 Lerp(Vector4 a, Vector4 b, float s)
		{
			return new Vector4(MathHelper.Lerp(a.X, b.X, s),
				MathHelper.Lerp(a.Y, b.Y, s),
				MathHelper.Lerp(a.Z, b.Z, s),
				MathHelper.Lerp(a.W, b.W, s));
		}

		public static Color ToColor(this Vector4 v)
		{
			return new Color((byte)(v.X * 255.0f), (byte)(v.Y * 255.0f), (byte)(v.Z * 255.0f), (byte)(v.W * 255.0f));
		}
	}
}
