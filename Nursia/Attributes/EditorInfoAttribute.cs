using System;

namespace Nursia.Attributes
{
	public class EditorInfoAttribute: Attribute
	{
		public string Category { get; }

		public EditorInfoAttribute(string category)
		{
			Category = category;
		}
	}
}
