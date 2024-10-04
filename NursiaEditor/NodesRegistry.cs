using Nursia;
using Nursia.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NursiaEditor
{
	internal static class NodesRegistry
	{
		private static readonly List<Assembly> _assemblies = new List<Assembly>();
		private static readonly Dictionary<string, List<Type>> _nodesByCategories = new Dictionary<string, List<Type>>();

		public static void AddAssembly(Assembly assembly)
		{
			if (_assemblies.Contains(assembly))
			{
				return;
			}

			foreach (var type in assembly.GetTypes())
			{
				var attr = type.GetCustomAttribute<EditorInfoAttribute>();
				if (attr == null)
				{
					return;
				}

				Nrs.LogInfo($"Adding node of type {type}");


			}

			_assemblies.Add(assembly);
		}
	}
}
