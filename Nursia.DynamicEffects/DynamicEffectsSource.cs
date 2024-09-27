using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Nursia.Rendering;
using System.Reflection;
using SharpDX.Multimedia;

namespace Nursia
{
	public class DynamicEffectsSource : IEffectsSource
	{
#if FNA
		private const string EffectsResourcePath = "FNA/bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		private class EffectSource
		{
			public string FilePath { get; }
			public DateTime LastWrite { get; set; }
			public List<EffectSource> Dependencies { get; } = new List<EffectSource>();

			public EffectSource(string filePath)
			{
				FilePath = filePath;
				LastWrite = File.GetLastWriteTime(filePath);
			}

			public override string ToString() => Path.GetFileName(FilePath);
		}

		private readonly string _folder;
		private readonly static Regex _includeRegex = new Regex(@"#include ""([\.\w]+)""");
		private readonly Dictionary<string, EffectSource> _sources = new Dictionary<string, EffectSource>();

		public DynamicEffectsSource(string folder)
		{
			_folder = folder ?? throw new ArgumentNullException(folder);
		}

		private static string BuildCompiledEffectName(string name, Dictionary<string, string> defines)
		{
			var sb = new StringBuilder();

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

			sb.Append(".efb");

			return sb.ToString();
		}

		private EffectSource AddSourceFile(string file)
		{
			if (!File.Exists(file))
			{
				throw new Exception($"Could not find '{file}'");
			}

			EffectSource source;
			if (_sources.TryGetValue(file, out source))
			{
				return source;
			}

			source = new EffectSource(file);
			_sources[file] = source;
			Nrs.LogInfo($"Added effect source file '{file}'");

			// Add dependencies
			var folder = Path.GetDirectoryName(file);
			var data = File.ReadAllText(file);
			var matches = _includeRegex.Matches(data);
			foreach (Match match in matches)
			{
				var includeFile = match.Groups[1].Value;

				Nrs.LogInfo($"'{file}' depends on '{includeFile}'");

				var includePath = Path.Combine(folder, includeFile);
				source.Dependencies.Add(AddSourceFile(includePath));
			}

			return source;
		}

		public Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines)
		{
			try
			{
				// Build effect source file name
				var assemblyName = assembly.GetName().Name;
				var sourceFilePath = Path.Combine(_folder, $"{assemblyName}/Effects/{name}.fx");
				AddSourceFile(sourceFilePath);

				// Check if precompiled version of the effect exists
				var binaryName = BuildCompiledEffectName(name, defines);
				var binaryPath = Path.Combine(_folder, $"{assembly.GetName().Name}/Effects/{EffectsResourcePath}/{binaryName}");
				var binaryVersionExists = File.Exists(binaryPath);
				if (binaryVersionExists)
				{
					Nrs.LogInfo($"Compiled version '{binaryPath}' exist");
				}
				else
				{
					Nrs.LogInfo($"Compiled version '{binaryPath}' doesn't exist");
				}

				byte[] effectData;
				if (binaryVersionExists)
				{
					Nrs.LogInfo("Using compiled version of the effect");

					effectData = File.ReadAllBytes(binaryPath);
				}
				else
				{
					var fullPath = Path.Combine(_folder, $"{assembly.GetName().Name}/Effects/{name}.fx");

					fullPath = Path.ChangeExtension(fullPath, "fx");
					var compilationResult = ShaderCompiler.Compile(fullPath, defines);
					effectData = compilationResult.Data;
				}

				var effect = new Effect(Nrs.GraphicsDevice, effectData);

				return effect;
			}
			catch (Exception ex)
			{
				Nrs.LogError(ex.Message);
				throw;
			}
		}
	}
}
