using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Nursia
{
	internal static class VariantsParser
	{
		private class DefinesBuilder
		{
			private List<Dictionary<string, string>> _currentDefine = new List<Dictionary<string, string>>();
			private List<List<Dictionary<string, string>>> _definesByLevel;
			private int _level;
			private List<Dictionary<string, string>> _result;

			private void StoreResult()
			{
				var dict = new Dictionary<string, string>();
				foreach (var defines in _currentDefine)
				{
					foreach (var pair in defines)
					{
						if (pair.Key == "_")
						{
							continue;
						}

						dict[pair.Key] = pair.Value;
					}
				}

				_result.Add(dict);
			}

			private void BuildInternal()
			{
				foreach (var defines in _definesByLevel[_level])
				{
					_currentDefine.Add(defines);

					if (_level < _definesByLevel.Count - 1)
					{
						++_level;
						BuildInternal();
						--_level;
					}
					else
					{
						// Store result at leafs
						StoreResult();
					}

					_currentDefine.RemoveAt(_currentDefine.Count - 1);
				}
			}

			public List<Dictionary<string, string>> Build(List<List<Dictionary<string, string>>> definesByLevel)
			{
				_definesByLevel = definesByLevel ?? throw new ArgumentNullException(nameof(definesByLevel));
				_result = new List<Dictionary<string, string>>();
				_level = 0;

				BuildInternal();

				return _result;
			}
		}


		public static List<Dictionary<string, string>> FromXml(string xml)
		{
			var definesByLevel = new List<List<Dictionary<string, string>>>();

			// First run: parse data
			var xDoc = XDocument.Parse(xml);
			foreach (var multiCompile in xDoc.Root.Elements())
			{
				var parts = multiCompile.Value.Split(";");

				var levelDefine = new List<Dictionary<string, string>>();
				foreach (var part in parts)
				{
					var partTrimmed = part.Trim();
					if (string.IsNullOrEmpty(partTrimmed))
					{
						continue;
					}

					if (!partTrimmed.StartsWith("["))
					{
						// Single value
						var parts2 = partTrimmed.Split("=");
						var key = parts2[0].Trim();

						var value = "1";
						if (parts2.Length > 1)
						{
							value = parts2[1].Trim();
						}

						levelDefine.Add(new Dictionary<string, string>() { [key] = value });
					}
					else
					{
						if (!partTrimmed.EndsWith("]"))
						{
							throw new Exception($"Multi-value '{partTrimmed}' doesnt end with ']'");
						}

						var values = new Dictionary<string, string>();
						partTrimmed = partTrimmed.Substring(1, partTrimmed.Length - 2).Trim();
						var parts2 = partTrimmed.Split(',');
						foreach (var part2 in parts2)
						{
							var partTrimmed2 = part2.Trim();

							var parts3 = partTrimmed2.Split("=");
							var key = parts3[0].Trim();

							var value = "1";
							if (parts3.Length > 1)
							{
								value = parts3[1].Trim();
							}

							values[key] = value;
						}

						levelDefine.Add(values);
					}

				}

				definesByLevel.Add(levelDefine);
			}

			// Second run: recursively build the result
			var definesBuilder = new DefinesBuilder();
			return definesBuilder.Build(definesByLevel);
		}
	}
}
