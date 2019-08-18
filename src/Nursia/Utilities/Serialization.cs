using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nursia.Utilities
{
	public static class Serialization
	{
		internal static float ToFloat(this object data)
		{
			return Convert.ToSingle(data, CultureInfo.InvariantCulture);
		}

		internal static string GetId(this Dictionary<string, object> data)
		{
			var result = string.Empty;

			object obj;
			if (data.TryGetValue("id", out obj) && obj != null)
			{
				result = obj.ToString();
			}

			return result;
		}

		internal static string EnsureString(this Dictionary<string, object> data, string key)
		{
			object obj;
			if (!data.TryGetValue(key, out obj))
			{
				throw new Exception(string.Format("Mandatory field '{0}' missing.", key));
			}

			return obj != null ? obj.ToString() : string.Empty;
		}
	}
}