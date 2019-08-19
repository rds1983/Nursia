using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nursia.Utilities
{
	internal static class Serialization
	{
		public static float ToFloat(this object data)
		{
			return Convert.ToSingle(data, CultureInfo.InvariantCulture);
		}

		public static Vector3 ToVector3(this object data)
		{
			var floats = (List<object>)data;
			return new Vector3(floats[0].ToFloat(),
				floats[1].ToFloat(),
				floats[2].ToFloat());
		}

		public static Vector4 ToVector4(this object data)
		{
			var floats = (List<object>)data;
			var result = new Vector4();
			result.X = floats[0].ToFloat();
			result.Y = floats[1].ToFloat();
			result.Z = floats[2].ToFloat();

			if (floats.Count > 3)
			{
				result.W = floats[3].ToFloat();
			}

			return result;
		}

		public static string GetId(this Dictionary<string, object> data)
		{
			var result = string.Empty;

			object obj;
			if (data.TryGetValue("id", out obj) && obj != null)
			{
				result = obj.ToString();
			}

			return result;
		}

		public static object EnsureObject(this Dictionary<string, object> data, string key)
		{
			object obj;
			if (!data.TryGetValue(key, out obj))
			{
				throw new Exception(string.Format("Mandatory field '{0}' missing.", key));
			}

			return obj;
		}

		public static string EnsureString(this Dictionary<string, object> data, string key)
		{
			var obj = EnsureObject(data, key);

			return obj != null ? obj.ToString() : string.Empty;
		}
	}
}