using System.Globalization;

namespace Nursia.ModelImporter
{
	static class Extensions
	{
		public static string Serialize(this float f)
		{
			return f.ToString(CultureInfo.InvariantCulture);
		}
	}
}
