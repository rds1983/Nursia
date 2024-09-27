using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

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
			public List<EffectSource> Dependencies { get; } = new List<EffectSource>();

			public EffectSource(string filePath)
			{
				FilePath = filePath;
			}

			public override string ToString() => Path.GetFileName(FilePath);
		}

		private class CompiledEffectInfo
		{
			public EffectSource Source;
			public Dictionary<string, string> Defines;
			public readonly Dictionary<string, DateTime> LastWrite = new Dictionary<string, DateTime>();
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

			file = file.Replace("\\", "/");

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

		private static void FillLastWrite(EffectSource source, Dictionary<string, DateTime> lastWrite)
		{
			lastWrite[source.FilePath] = File.GetLastWriteTime(source.FilePath);

			foreach (var dep in source.Dependencies)
			{
				FillLastWrite(dep, lastWrite);
			}
		}

		public Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines)
		{
			try
			{
				// Build effect source file name
				var assemblyName = assembly.GetName().Name;
				var sourceFilePath = Path.Combine(_folder, $"{assemblyName}/Effects/{name}.fx");
				var source = AddSourceFile(sourceFilePath);

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
					var compilationResult = ShaderCompiler.Compile(sourceFilePath, defines);
					effectData = compilationResult.Data;
				}

				var effect = new Effect(Nrs.GraphicsDevice, effectData);

				var effectInfo = new CompiledEffectInfo
				{
					Source = source,
					Defines = defines
				};

				FillLastWrite(source, effectInfo.LastWrite);
				effect.Tag = effectInfo;

				return effect;
			}
			catch (Exception ex)
			{
				Nrs.LogError(ex.Message);
				throw;
			}
		}

		public bool IsEffectValid(Effect effect)
		{
			var effectInfo = (CompiledEffectInfo)effect.Tag;
			var lastWrite = effectInfo.LastWrite;
			foreach (var pair in lastWrite)
			{
				var newLastWrite = File.GetLastWriteTime(pair.Key);
				if (pair.Value != newLastWrite)
				{
					return false;
				}
			}

			return true;
		}

		public Effect UpdateEffect(Effect effect)
		{
			var effectInfo = (CompiledEffectInfo)effect.Tag;
			var compilationResult = ShaderCompiler.Compile(effectInfo.Source.FilePath, effectInfo.Defines);
			var effectData = compilationResult.Data;

			var newEffect = new Effect(Nrs.GraphicsDevice, effectData);

			effectInfo.LastWrite.Clear();
			FillLastWrite(effectInfo.Source, effectInfo.LastWrite);
			newEffect.Tag = effectInfo;

			return newEffect;
		}
	}
}