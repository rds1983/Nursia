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
		private static readonly SortedDictionary<string, List<Type>> _nodesByCategories = new SortedDictionary<string, List<Type>>();

		public static IReadOnlyDictionary<string, List<Type>> NodesByCategories => _nodesByCategories;

		public static void AddAssembly(Assembly assembly)
		{
			if (_assemblies.Contains(assembly))
			{
				return;
			}

			foreach (var type in assembly.GetTypes())
			{
				var attr = type.GetCustomAttribute<EditorInfoAttribute>(true);
				if (attr == null)
				{
					continue;
				}

				Nrs.LogInfo($"Adding node of type {type}");

				List<Type> types;
				if (!_nodesByCategories.TryGetValue(attr.Category, out types))
				{
					types = new List<Type>();
					_nodesByCategories[attr.Category] = types;
				}

				types.Add(type);
			}

			_assemblies.Add(assembly);
		}
	}
}
