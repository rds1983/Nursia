using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.ModelImporter.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nursia.ModelImporter
{
	class Exporter
	{
		private const string IdName = "name";

		private int _level = 0;
		private ModelContent _scene;

		private class BracersScope : IDisposable
		{
			private readonly string _openBracerSymbol;
			private readonly string _closeBracerSymbol;
			private readonly Exporter _exporter;

			public BracersScope(Exporter exporter, string openBracerSymbol, string closeBracerSymbol, bool indent = true)
			{
				_exporter = exporter;
				_openBracerSymbol = openBracerSymbol;
				_closeBracerSymbol = closeBracerSymbol;

				if (indent)
				{
					_exporter.WriteIndent();
				}
				_exporter.WriteLine(_openBracerSymbol.ToString());
				++_exporter._level;
			}

			public void Dispose()
			{
				--_exporter._level;
				_exporter.WriteLineWithIndent(_closeBracerSymbol.ToString());
			}
		}

		private readonly StringBuilder _builder = new StringBuilder();

		public Exporter()
		{
		}

		private void Write(string s)
		{
			_builder.Append(s);
		}

		private void WriteIndent()
		{
			for (var i = 0; i < _level; ++i)
			{
				Write("\t");
			}
		}

		private void WriteLine(string s)
		{
			Write(s);
			Write("\n");
		}

		private void WriteLineWithIndent(string s)
		{
			WriteIndent();
			WriteLine(s);
		}

		private void WriteWithIndent(string s)
		{
			WriteIndent();
			Write(s);
		}

		private BracersScope CreateCurlyBracersScope(bool indent = true, bool hasComma = false)
		{
			return new BracersScope(this, "{", hasComma ? "}," : "}", indent);
		}

		private BracersScope CreateSquareBracersScope(bool indent = true, bool hasComma = false)
		{
			return new BracersScope(this, "[", hasComma ? "]," : "]", indent);
		}

		private void WriteSimpleProperty(string name, string value, bool hasComma = true)
		{
			WritePropertyStart(name);
			WriteLine("\"" + value + "\"" + (hasComma ? "," : string.Empty));
		}

		private void WritePropertyStart(string name)
		{
			WriteIndent();
			Write("\"" + name + "\": ");
		}

		private void WriteFloatArray(bool hasComma, params float[] data)
		{
			using (var materialScope = CreateSquareBracersScope(false, hasComma))
			{
				WriteIndent();
				for (var k = 0; k < data.Length; ++k)
				{
					Write(data[k].Serialize());
					if (k < data.Length - 1)
					{
						Write(", ");
					}
				}
				WriteLine(string.Empty);
			}
		}

		private void WriteMatrix(Matrix m, bool hasComma = true)
		{
			WriteFloatArray(hasComma, m[0], m[1], m[2], m[3],
				m[4], m[5], m[6], m[7],
				m[8], m[9], m[10], m[11],
				m[12], m[13], m[14], m[15]);
		}

		private void WriteVector3(Vector3 v, bool hasComma = true)
		{
			WriteFloatArray(hasComma, v.X, v.Y, v.Z);
		}

		private void TraverseTree(BoneContent root, Action<BoneContent> action)
		{
			if (root == null)
			{
				return;
			}

			action(root);

			foreach (var child in root.Children)
			{
				TraverseTree(child, action);
			}
		}

		private void WriteDeclaration(VertexDeclaration vertexDeclaration)
		{
			WritePropertyStart("declaration");

			using (var declarationScope = CreateSquareBracersScope(false, true))
			{
				var elements = vertexDeclaration.GetVertexElements();
				for(var i = 0; i < elements.Length; ++i)
				{
					var element = elements[i];
					WriteIndent();
					using (var elementScope = CreateCurlyBracersScope(false, i < elements.Length - 1))
					{
						WriteSimpleProperty("offset", element.Offset.ToString());
						WriteSimpleProperty("format", element.VertexElementFormat.ToString());
						WriteSimpleProperty("usage", element.VertexElementUsage.ToString());
					}
				}
			}
		}

		private void WriteMeshes()
		{
			var totalParts = 0;
			foreach (var mesh in _scene.Meshes)
			{
				foreach (var part in mesh.Parts)
				{
					++totalParts;
				}
			}

			WritePropertyStart("meshes");
			using (var meshesScope = CreateSquareBracersScope(false, true))
			{
				var idx = 0;
				foreach(var mesh in _scene.Meshes)
				{
					foreach (var part in mesh.Parts)
					{
						using (var partScope = CreateCurlyBracersScope(true, idx < totalParts))
						{
							WriteSimpleProperty("meshNodeName", mesh.Name);
							WriteDeclaration(part.VertexDeclaration);
							WriteSimpleProperty("elementsPerRow", part.ElementsPerRow.ToString());
							WritePropertyStart("indices");
							using (var indicesScope = CreateSquareBracersScope(false, true))
							{
								WriteIndent();
								for (var j = 0; j < part.Indices.Count; ++j)
								{
									Write(part.Indices[j].ToString());
									if (j < part.Indices.Count - 1)
									{
										Write(", ");

										if (j > 0 && j % 20 == 0)
										{
											WriteLine(string.Empty);
											WriteIndent();
										}
									}
								}
								WriteLine(string.Empty);
							}
							WritePropertyStart("vertices");
							using (var verticesScope = CreateSquareBracersScope(false, true))
							{
								WriteIndent();

								var cnt = 0;
								var size = part.Vertices.GetLength(0) * part.Vertices.GetLength(1);
								for (var j = 0; j < part.Vertices.GetLength(0); ++j)
								{
									for (var k = 0; k < part.Vertices.GetLength(1); ++k)
									{
										Write(part.Vertices[j, k].Serialize());
										if (cnt < size - 1)
										{
											Write(", ");
										}

										++cnt;
									}
									WriteLine(string.Empty);
									if (j < part.Vertices.GetLength(0) - 1)
									{
										WriteIndent();
									}
								}
							}
							WriteSimpleProperty("material", part.Material.Name, false);
						}

						++idx;
					}
				}
			}
		}

		private void WriteMaterials()
		{
			WritePropertyStart("materials");
			using (var materialsScope = CreateSquareBracersScope(false, true))
			{
				foreach (var material in _scene.Materials)
				{
					using (var materialScope = CreateCurlyBracersScope(true, true))
					{
						WriteSimpleProperty(IdName, material.Name);

						WriteSimpleProperty("texture", material.Texture != null ? material.Texture.FilePath : string.Empty);
					}
				}
			}
		}

		private void WriteNodes(BoneContent root)
		{
			WriteSimpleProperty(IdName, root.Name);
			WritePropertyStart("transform");
			WriteMatrix(root.Transform);

			WriteSimpleProperty("boneId", root.BoneId.ToString());

			if (root.Children.Count > 0)
			{
				WritePropertyStart("children");
				using (var nodesScope = CreateSquareBracersScope(false, false))
				{
					foreach (var child in root.Children)
					{
						if (child is MeshContent)
						{
							continue;
						}

						WriteIndent();
						using (var nodeScope = CreateCurlyBracersScope(false, true))
						{
							WriteNodes(child);
						}
					}
				}
			}
		}

		public string Export(ModelContent scene)
		{
			_builder.Clear();
			_scene = scene;

			using (var topScope = CreateCurlyBracersScope())
			{
				WriteSimpleProperty("version", Nrs.N3tVersion);
				WriteMeshes();
				WriteMaterials();

				WritePropertyStart("rootBone");
				using (var nodesScope = CreateCurlyBracersScope(false, false))
				{
					WriteNodes(scene.RootBone);
				}
			}

			return _builder.ToString();
		}
	}
}