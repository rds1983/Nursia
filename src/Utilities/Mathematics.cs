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
	}
}
