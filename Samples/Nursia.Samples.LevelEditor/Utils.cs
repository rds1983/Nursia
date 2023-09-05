using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Nursia.Samples.LevelEditor
{
	public static class Utils
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

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

		public static BoundingBox CreateBoundingBox(float x1, float x2, float y1, float y2, float z1, float z2)
		{
			var min = new Vector3(Math.Min(x1, x2), Math.Min(y1, y2), Math.Min(z1, z2));
			var max = new Vector3(Math.Max(x1, x2), Math.Max(y1, y2), Math.Max(z1, z2));

			return new BoundingBox(min, max);
		}
	}
}