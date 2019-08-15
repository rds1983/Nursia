using System.Collections.Generic;
using System.Globalization;

namespace Nursia.Utilities
{
	public static class Serialization
	{
		internal static float ToFloat(this object data)
		{
			string s = data.ToString();
			return float.Parse(s, CultureInfo.InvariantCulture);
		}
	}
}