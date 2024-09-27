using Nursia.Rendering;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Nursia
{
	public static class EffectsRegistry
	{
		private class StoredEffect
		{
			public EffectBinding EffectBinding;
			public Assembly Assembly;
			public string Name;
			public Dictionary<string, string> Defines;
		}

		private static Dictionary<string, StoredEffect> _storedEffects = new Dictionary<string, StoredEffect>();

		private static string BuildEffectKey(Assembly assembly, string name, Dictionary<string, string> defines = null)
		{
			var sb = new StringBuilder();

			var assemblyName = assembly.GetName().Name;

			sb.Append(assemblyName);
			sb.Append("/");
			sb.Append(name);

			if (defines != null && defines.Count > 0)
			{
				var keys = (from def in defines.Keys orderby def select def).ToArray();
				for (var i = 0; i < keys.Length; ++i)
				{
					sb.Append("_");

					var k = keys[i];
					sb.Append(k);
					var value = defines[k];
					if (value != "1")
					{
						sb.Append("_");
						sb.Append(value);
					}
				}
			}

			return sb.ToString();
		}

		public static T GetEffectBinding<T>(Assembly assembly, string name, Dictionary<string, string> defines = null)
			where T : EffectBinding, new()
		{
			var key = BuildEffectKey(assembly, name, defines);

			StoredEffect result;
			if (_storedEffects.TryGetValue(key, out result))
			{
				return (T)result.EffectBinding;
			}

			var effect = Nrs.EffectsSource.GetEffect(assembly, name, defines);
			var binding = new T
			{
				Effect = effect
			};

			result = new StoredEffect
			{
				EffectBinding = binding,
				Assembly = assembly,
				Name = name,
				Defines = defines,
			};

			_storedEffects[key] = result;

			return binding;
		}

		public static T GetStockEffectBinding<T>(string name, Dictionary<string, string> defines = null)
				where T : EffectBinding, new()
		{
			return GetEffectBinding<T>(typeof(EffectsRegistry).Assembly, name, defines);
		}
	}
}
