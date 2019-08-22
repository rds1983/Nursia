using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniJSON;
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

		public static Sprite3D LoadFromJson(string json, Func<string, Texture2D> textureGetter)
		{
			var root = (Dictionary<string, object>)Json.Deserialize(json);

			var result = new Sprite3D();
			var meshesData = (List<object>)root["meshes"];
			var meshes = new Dictionary<string, Mesh>();
			foreach (Dictionary<string, object> meshData in meshesData)
			{
				// Determine vertex type
				var declarationTypeName = (string)meshData["declaration"];

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
				var vertices = (List<object>)meshData["vertices"];
				var vertexBuffer = loader.CreateVertexBuffer(vertices);

				var parts = (List<object>)meshData["parts"];
				foreach (Dictionary<string, object> partData in parts)
				{
					var id = partData.GetId();
					var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
					var indicesData = (List<object>)partData["indices"];
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

					meshes[id] = mesh;
				}
			}

			var materials = (List<object>)root["materials"];
			foreach (Dictionary<string, object> materialData in materials)
			{
				var material = new Material
				{
					Id = materialData.GetId(),
					DiffuseColor = Color.White
				};

				object obj;
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

			var nodesData = (List<object>)root["nodes"];
			foreach (Dictionary<string, object> nodeData in nodesData)
			{
				var meshNode = new MeshNode
				{
					Id = nodeData.GetId()
				};

				if (!nodeData.ContainsKey("parts"))
				{
					continue;
				}

				var partsData = (List<object>)nodeData["parts"];
				foreach (Dictionary<string, object> partData in partsData)
				{
					var material = (from m in result.Materials where m.Id == (string)partData["materialid"] select m).First();

					var meshPart = new MeshNodePart
					{
						Mesh = meshes[partData["meshpartid"].ToString()],
						Material = material
					};

					meshNode.Parts.Add(meshPart);
				}

				result.Meshes.Add(meshNode);
			}

			return result;
		}
	}
}
