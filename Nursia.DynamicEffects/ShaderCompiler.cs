using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nursia
{
	internal static class ShaderCompiler
	{
		class IncludeFX : Include
		{
			private readonly string _folder;

			public IncludeFX(string folder)
			{
				_folder = folder;
			}

			public IDisposable Shadow { get; set; }

			public void Close(Stream stream)
			{
				stream.Close();
				stream.Dispose();
			}

			public void Dispose()
			{
			}

			public Stream Open(IncludeType type, string fileName, Stream parentStream)
			{
				return new FileStream(Path.Combine(_folder, fileName), FileMode.Open);
			}
		}

		private static ShaderMacro[] ToMacroses(Dictionary<string, string> defines)
		{
			var result = new List<ShaderMacro>();

			if (defines != null)
			{
				var orderedKeys = from k in defines.Keys
								  where !string.IsNullOrEmpty(k)
								  orderby k
								  select k;

				foreach (var k in orderedKeys)
				{
					result.Add(new ShaderMacro(k, defines[k]));
				}
			}

			return result.ToArray();
		}

		public static CompilationResult Compile(string inputFile, Dictionary<string, string> defines, Action<string> reporter)
		{
			var folder = Path.GetDirectoryName(inputFile);
			var sb = new StringBuilder();
			sb.Clear();
			sb.Append($"Compiling {inputFile}");

			if (defines != null && defines.Count > 0)
			{
				sb.Append("with defines: ");
				var i = 0;
				foreach (var pair in defines)
				{
					sb.Append($"{pair.Key} = {pair.Value}");
					if (i < defines.Count - 1)
					{
						sb.Append($", ");
					}

					++i;
				}
			}

			reporter(sb.ToString());
			var data = File.ReadAllText(inputFile);
			var compiled = ShaderBytecode.Compile(data, "fx_2_0", ShaderFlags.OptimizationLevel3,
				EffectFlags.None, ToMacroses(defines), new IncludeFX(folder));

			if (!string.IsNullOrEmpty(compiled.Message))
			{
				reporter(compiled.Message);
			}
			return new CompilationResult(compiled.Bytecode, defines);
		}

		public static CompilationResult[] Compile(string inputFile, Action<string> reporter)
		{
			var xmlFile = Path.ChangeExtension(inputFile, "xml");
			List<Dictionary<string, string>> variants;
			if (File.Exists(xmlFile))
			{
				var xml = File.ReadAllText(xmlFile);
				variants = VariantsParser.FromXml(xml);
			}
			else
			{
				variants = new List<Dictionary<string, string>>
				{
					new Dictionary<string, string>()
				};
			}

			var result = new List<CompilationResult>();
			foreach (var defines in variants)
			{
				var compilationResult = Compile(inputFile, defines, reporter);
				result.Add(compilationResult);
			}

			return result.ToArray();
		}
	}
}