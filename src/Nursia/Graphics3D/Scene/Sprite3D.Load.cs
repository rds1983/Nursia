using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Nursia.Graphics3D.Utils.Vertices;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nursia.Graphics3D.Scene
{
	partial class Sprite3D
	{
		private const string IdName = "id";

		private readonly static Dictionary<Type, Type> _loaders = new Dictionary<Type, Type>();

		static Sprite3D()
		{
			_loaders[typeof(VertexPositionNormalTexture)] = typeof(VertexPositionNormalTextureLoader);
			_loaders[typeof(VertexPositionNormalTextureBlend)] = typeof(VertexPositionNormalTextureBlendLoader);
		}

		private static Stream EnsureOpen(Func<string, Stream> streamOpener, string name)
		{
			var result = streamOpener(name);
			if (result == null)
			{
				throw new Exception(string.Format("stream is null for name '{0}'", name));
			}

			return result;
		}

		private static Node LoadNode(JObject data, Dictionary<string, List<MeshNodePart>> meshes)
		{
			Node result = null;

			var type = data["type"].ToString();

			if (type == "mesh")
			{
				var meshNode = new MeshNode();

				var parts = meshes[data[IdName].ToString()];
				meshNode.Parts.AddRange(parts);

				result = meshNode;
			} else
			{
				result = new Node();
			}

			result.Id = data.GetId();
			result.Transform = data["transform"].ToMatrix();

			if (data.ContainsKey("children"))
			{
				var children = (JArray)data["children"];
				foreach (JObject child in children)
				{
					var childNode = LoadNode(child, meshes);
					result.Children.Add(childNode);
				}
			}

			return result;
		}

		public static Sprite3D LoadFromJson(string json, Func<string, Texture2D> textureGetter)
		{
			var root = JObject.Parse(json);

			var result = new Sprite3D();
			var meshesData = (JArray)root["meshes"];
			var meshes = new Dictionary<string, List<MeshNodePart>>();
			foreach (JObject meshData in meshesData)
			{
				var id = meshData.GetId();
				var parts = (JArray)meshData["parts"];
				foreach (JObject partData in parts)
				{
					// Determine vertex type
					var declarationTypeName = partData["declaration"].ToString();

					// Firstly try MonoGame/FNA assembly
					var declarationType = typeof(VertexPosition).Assembly.GetType(declarationTypeName);

					if (declarationType == null)
					{
						// Now Nursia
						declarationType = typeof(VertexPositionNormalTextureBlend).Assembly.GetType(declarationTypeName);
						if (declarationType == null)
						{
							// Not found
							throw new Exception(string.Format("Could not recognize vertex type '{0}'", declarationTypeName));
						}
					}

					Type loaderType;
					if (!_loaders.TryGetValue(declarationType, out loaderType))
					{
						throw new Exception(string.Format("Loader for vertex type '{0}' isn't registered", declarationTypeName));
					}

					var loader = (IVertexLoader)Activator.CreateInstance(loaderType);
					var vertices = (JArray)partData["vertices"];

					var vertexBuffer = loader.CreateVertexBuffer(vertices);

					// var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
					var type = PrimitiveType.TriangleList;
					var indicesData = (JArray)partData["indices"];
					var indices = new short[indicesData.Count];
					for (var i = 0; i < indicesData.Count; ++i)
					{
						indices[i] = Convert.ToInt16(indicesData[i]);
					}

					var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits,
						indices.Length, BufferUsage.None);
					indexBuffer.SetData(indices);

					var mesh = new Mesh
					{
						PrimitiveType = type,
						IndexBuffer = indexBuffer,
						VertexBuffer = vertexBuffer
					};

					List<MeshNodePart> m;
					if (!meshes.TryGetValue(id, out m))
					{
						m = new List<MeshNodePart>();
						meshes[id] = m;
					}

					var part = new MeshNodePart
					{
						Mesh = mesh,
						MaterialName = partData["material"].ToString()
					};
					m.Add(part);
				}
			}

			var materials = (JArray)root["materials"];
			foreach (JObject materialData in materials)
			{
				var material = new Material
				{
					Id = materialData.GetId(),
					DiffuseColor = Color.White
				};

				JToken obj;
				if (materialData.TryGetValue("diffuse", out obj) && obj != null)
				{
					material.DiffuseColor = new Color(obj.ToVector4(1.0f));
				}

				if (materialData.TryGetValue("texture", out obj) && obj != null)
				{
					var name = obj.ToString();
					if (!string.IsNullOrEmpty(name))
					{
						material.Texture = textureGetter(name);
					}
				}

				result.Materials.Add(material);
			}

			// Set meshes materials
			foreach(var ml in meshes)
			{
				foreach(var m in ml.Value)
				{
					m.Material = (from mt in result.Materials where mt.Id == m.MaterialName select mt).First();
				}
			}


			var rootNode = LoadNode((JObject)root["rootNode"], meshes);

			result.Children.Add(rootNode);

			return result;
		}
	}
}
