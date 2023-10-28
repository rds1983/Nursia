using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Nursia
{
	public class FolderWatcher : IExternalEffectsSource
	{
		private class EffectWrapper
		{
			public Effect Effect;
		}


		private readonly string _folder;
		private readonly Dictionary<string, EffectWrapper> _effects = new Dictionary<string, EffectWrapper>();


		public FolderWatcher(string folder)
		{
			_folder = folder ?? throw new ArgumentNullException(folder);
		}

		private static string BuildKey(string name, Dictionary<string, string> defines)
		{
			var key = new StringBuilder();

			key.Append(name);

			if (defines != null && defines.Count > 0)
			{
				var keys = (from def in defines.Keys orderby def select def).ToArray();
				foreach (var k in keys)
				{
					key.Append("_");
					key.Append(k);
					var value = defines[k];
					if (value != "1")
					{
						key.Append("_");
						key.Append(value);
					}
				}
			}

			return key.ToString();
		}


		public Func<Effect> Get(GraphicsDevice graphicsDevice, string name, Dictionary<string, string> defines)
		{
			var key = BuildKey(name, defines);
			EffectWrapper wr;
			if (!_effects.TryGetValue(key, out wr))
			{
				wr = new EffectWrapper();
				_effects[key] = wr;
			}

			return () =>
			{
				if (wr.Effect == null)
				{
					var fullPath = Path.Combine(_folder, name);
					fullPath = Path.ChangeExtension(fullPath, "fx");

					var compilationResult = ShaderCompiler.Compile(fullPath, defines, s => Console.WriteLine(s));
					wr.Effect = new Effect(graphicsDevice, compilationResult.Data);
				}

				return wr.Effect;
			};
		}
	}
}
