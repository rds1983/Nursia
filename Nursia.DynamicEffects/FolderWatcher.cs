using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Nursia
{
	public class FolderWatcher
	{
		private class EffectWrapper
		{
			public bool FirstLoad;
			public Effect Effect;
			public Effect OldEffect;
		}

		public string BinaryFolder { get; set; }

		private readonly string _folder;
		private readonly static Regex _includeRegex = new Regex(@"#include ""([\.\w]+)""");
		private readonly Dictionary<string, HashSet<string>> _dependencies = new Dictionary<string, HashSet<string>>();
		private readonly Dictionary<string, EffectWrapper> _effects = new Dictionary<string, EffectWrapper>();
		private readonly FileSystemWatcher _watcher;

		public FolderWatcher(string folder)
		{
			_folder = folder ?? throw new ArgumentNullException(folder);

			// Build dependencies
			var files = Directory.EnumerateFiles(_folder, "*.fx");
			foreach (var file in files)
			{
				var data = File.ReadAllText(file);
				var matches = _includeRegex.Matches(data);
				foreach (Match match in matches)
				{
					var includeFile = match.Groups[1].Value;

					HashSet<string> deps;
					if (!_dependencies.TryGetValue(includeFile, out deps))
					{
						deps = new HashSet<string>();
						_dependencies[includeFile] = deps;
					}

					deps.Add(file);
				}
			}

			_watcher = new FileSystemWatcher
			{
				Path = folder,
				NotifyFilter = NotifyFilters.LastWrite,
				Filter = "*.*"
			};

			_watcher.Changed += Watcher_Changed;
			_watcher.EnableRaisingEvents = true;
		}

		private void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed)
			{
				return;
			}

			var file = Path.GetFileNameWithoutExtension(e.FullPath);
			foreach (var pair in _effects)
			{
				if (pair.Key.StartsWith(file))
				{
					if (pair.Value.Effect != null)
					{
						pair.Value.OldEffect = pair.Value.Effect;
					}

					pair.Value.Effect = null;
				}
			}
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
					if (!wr.FirstLoad && !string.IsNullOrEmpty(BinaryFolder))
					{
						var efbPath = Path.Combine(BinaryFolder, key);
						efbPath = Path.ChangeExtension(efbPath, "efb");
						if (File.Exists(efbPath))
						{
							wr.Effect = new Effect(graphicsDevice, File.ReadAllBytes(efbPath));
						}

						wr.FirstLoad = true;
					}

					if (wr.Effect == null)
					{
						var fullPath = Path.Combine(_folder, name);
						fullPath = Path.ChangeExtension(fullPath, "fx");
						try
						{
							var compilationResult = ShaderCompiler.Compile(fullPath, defines, s => Console.WriteLine(s));
							wr.Effect = new Effect(graphicsDevice, compilationResult.Data);

							if (!string.IsNullOrEmpty(BinaryFolder))
							{
								var efbPath = Path.Combine(BinaryFolder, key);
								efbPath = Path.ChangeExtension(efbPath, "efb");

								File.WriteAllBytes(efbPath, compilationResult.Data);
							}
						}
						catch (SharpDX.CompilationException ex)
						{
							// Try to restore previous version of the effect
							if (wr.OldEffect == null)
							{
								throw new Exception($"Error compiling {fullPath}. Can't restore from the previous version of effect too.", ex);
							}

							wr.Effect = wr.OldEffect;
							throw;
						}
					}
				}

				return wr.Effect;
			};
		}
	}
}
