using System.Globalization;

namespace Nursia.ModelImporter
{
	static class Extensions
	{
		public static string Serialize(this object f)
		{
			if (f == null)
			{
				return "0";
			}

			if (f is float)
			{
				return ((float)f).ToString(CultureInfo.InvariantCulture);
			}

			return f.ToString();
		}
	}
}
