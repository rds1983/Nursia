using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.ModelImporter.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nursia.ModelImporter
{
	class Exporter
	{
		private const string IdName = "name";

		private int _level = 0;
		private ModelContent _scene;

		private static Dictionary<string, object> CreateObject()
		{
			return new Dictionary<string, object>();
		}

		private static List<object> CreateList()
		{
			return new List<object>();
		}

		private List<object> BuildDeclaration(VertexDeclaration vertexDeclaration)
		{
			var result = CreateList();
			var elements = vertexDeclaration.GetVertexElements();
			for (var i = 0; i < elements.Length; ++i)
			{
				var element = elements[i];
				var elementData = CreateObject();
				elementData["offset"] = element.Offset;
				elementData["format"] = element.VertexElementFormat;
				elementData["usage"] = element.VertexElementUsage;

				result.Add(elementData);
			}

			return result;
		}

		private Dictionary<string, object> BuildBone(BoneContent root)
		{
			var result = CreateObject();

			result[IdName] = root.Name;
			result["boneId"] = root.BoneId;

			Vector3 translation, scale;
			Quaternion rotation;
			root.Transform.Decompose(out scale, out rotation, out translation);
			result["translate"] = translation;
			result["scale"] = scale;
			result["rotation"] = rotation.ToVector4();

			return result;
		}

		private List<object> BuildMeshes()
		{
			var result = CreateList();

			foreach (var mesh in _scene.Meshes)
			{
				var meshData = CreateObject();
				meshData["meshNodeName"] = mesh.Name;
				meshData["declaration"] = BuildDeclaration(mesh.VertexDeclaration);
				meshData["elementsPerRow"] = mesh.ElementsPerRow;
				meshData["bonesCount"] = mesh.BonesCount;

				var indicesData = CreateList();
				for (var j = 0; j < mesh.Indices.Count; ++j)
				{
					indicesData.Add(mesh.Indices[j]);
				}

				meshData["indices"] = indicesData;

				var verticesData = CreateList();
				for (var j = 0; j < mesh.Vertices.GetLength(0); ++j)
				{
					for (var k = 0; k < mesh.Vertices.GetLength(1); ++k)
					{
						verticesData.Add(mesh.Vertices[j, k]);
					}
				}
				meshData["vertices"] = verticesData;
				meshData["material"] = mesh.Material.Name;

				var bonesData = CreateList();
				foreach(var bone in mesh.Bones)
				{
					bonesData.Add(BuildBone(bone));
				}

				meshData["bones"] = bonesData;

				result.Add(meshData);
			}

			return result;
		}

		private List<object> BuildMaterials()
		{
			var result = CreateList();
			foreach (var material in _scene.Materials)
			{
				var materialData = CreateObject();
				materialData[IdName] = material.Name;
				materialData["texture"] = material.Texture != null ? material.Texture.FilePath : string.Empty;

				result.Add(materialData);
			}

			return result;
		}

		private class BracersScope : IDisposable
		{
			private readonly string _openBracerSymbol;
			private readonly string _closeBracerSymbol;
			private readonly Exporter _exporter;
			private readonly StringBuilder _builder;
			private readonly bool _indent;

			public BracersScope(Exporter exporter, 
				StringBuilder builder, 
				string openBracerSymbol, 
				string closeBracerSymbol,
				bool indent)
			{
				_builder = builder;
				_openBracerSymbol = openBracerSymbol;
				_closeBracerSymbol = closeBracerSymbol;
				_exporter = exporter;
				_indent = indent;

				if (indent)
				{
					_builder.AppendLine(_openBracerSymbol.ToString());
				} else
				{
					_builder.Append(_openBracerSymbol.ToString() + " ");
				}

				++_exporter._level;
			}

			public void Dispose()
			{
				--_exporter._level;

				if (_indent)
				{
					_builder.AppendLine();
					_builder.Append(_exporter.BuildIndent());
					_builder.Append(_closeBracerSymbol.ToString());
				} else
				{
					_builder.Append(" " + _closeBracerSymbol.ToString());
				}
			}
		}

		private string BuildIndent()
		{
			if (_level == 0)
			{
				return string.Empty;
			}

			return new string('\t', _level);
		}

		private BracersScope CreateCurlyBracersScope(StringBuilder sb)
		{
			return new BracersScope(this, sb, "{", "}", true);
		}

		private BracersScope CreateSquareBracersScope(StringBuilder sb, bool indent)
		{
			return new BracersScope(this, sb, "[", "]", indent);
		}

		private string WriteObject(object data, int? elementsPerRow = null, bool indent = true)
		{
			var asObject = data as Dictionary<string, object>;
			if (asObject != null)
			{
				var sb = new StringBuilder();

				using (var scope = CreateCurlyBracersScope(sb))
				{
					var values = new List<string>();
					foreach(var pair in asObject)
					{
						var epr = default(int?);

						if (pair.Key == "indices")
						{
							epr = 20;
						} else if (pair.Key == "vertices")
						{
							epr = (int)asObject["elementsPerRow"];
						}

						var value = string.Format("\"{0}\" : {1}", 
							pair.Key, 
							WriteObject(pair.Value, epr));
						values.Add(BuildIndent() + value);
					}

					sb.Append(string.Join(",\n", values));
				}

				return sb.ToString();
			}

			var asList = data as List<object>;
			if (asList != null)
			{
				var isPrimitive = asList.Count > 0 && asList[0].GetType().IsPrimitive;
				var sb = new StringBuilder();

				using (var scope = CreateSquareBracersScope(sb, indent))
				{
					var values = new List<string>();
					foreach (var pair in asList)
					{
						if (isPrimitive)
						{
							values.Add(WriteObject(pair));
						}
						else
						{
							values.Add(BuildIndent() + WriteObject(pair));
						}
					}

					if (isPrimitive)
					{
						if (indent)
						{
							sb.Append(BuildIndent());
						}
						for(var i = 0; i < values.Count; ++i)
						{
							if (elementsPerRow != null &&
								i > 0 &&
								i % elementsPerRow.Value == 0)
							{
								sb.AppendLine();
								sb.Append(BuildIndent());
							}

							sb.Append(values[i]);
							if (i < values.Count - 1)
							{
								sb.Append(", ");
							}
						}
					} else
					{
						sb.Append(string.Join(",\n", values));
					}
				}

				return sb.ToString();
			}

			if (data == null)
			{
				return "\"" + string.Empty + "\"";
			}

			var asString = data as string;
			if (asString != null)
			{
				return "\"" + asString + "\"";
			}

			if (data.GetType().IsEnum)
			{
				return "\"" + data.ToString() + "\"";
			}

			if (data is Vector3)
			{
				var v = (Vector3)data;
				return WriteObject(new List<object>(new object[]{
					v.X,
					v.Y,
					v.Z
				}), null, false);
			}

			if (data is Vector4)
			{
				var v = (Vector4)data;
				return WriteObject(new List<object>(new object[]{
					v.X,
					v.Y,
					v.Z,
					v.W
				}), null, false);
			}

			if (data is float)
			{
				return ((float)data).ToString(CultureInfo.InvariantCulture);
			}

			return data.ToString();
		}

		public string Export(ModelContent scene)
		{
			_scene = scene;

			// First step: build output
			var output = new Dictionary<string, object>
			{
				["version"] = Nrs.N3tVersion,
				["meshes"] = BuildMeshes(),
				["materials"] = BuildMaterials()
			};

			// Second step: serialize to string
			return WriteObject(output);
		}
	}
}