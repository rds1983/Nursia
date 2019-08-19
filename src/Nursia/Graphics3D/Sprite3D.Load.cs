using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniJSON;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nursia.Graphics3D
{
	partial class Sprite3D
	{
		private readonly static Dictionary<Type, Type> _loaders = new Dictionary<Type, Type>();

		static Sprite3D()
		{
			_loaders[typeof(VertexPositionNormalTexture)] = typeof(VertexPositionNormalTextureLoader);
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

		public static Sprite3D LoadFromJson(string json, Func<string, Stream> streamOpener)
		{
			var graphicsAssembly = typeof(VertexPosition).Assembly;
			var vertexName = typeof(VertexPosition).Namespace;

			var root = (Dictionary<string, object>)Json.Deserialize(json);

			var result = new Sprite3D();
			var meshes = (List<object>)root["meshes"];
			foreach (Dictionary<string, object> meshData in meshes)
			{
				// Determine vertex type
				var declarationTypeName = (string)meshData["declaration"];
				var declarationType = graphicsAssembly.GetType(vertexName + "." + declarationTypeName);

				if (declarationType == null)
				{
					throw new Exception(string.Format("Could not recognize vertex type '{0}'", declarationTypeName));
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
						Id = id,
						PrimitiveType = type,
						IndexBuffer = indexBuffer,
						VertexBuffer = vertexBuffer
					};

					result.Meshes.Add(mesh);
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
					material.DiffuseColor = new Color(obj.ToVector4());
				}

				if (materialData.TryGetValue("texture", out obj) && obj != null)
				{
					var name = obj.ToString();
					if (!string.IsNullOrEmpty(name))
					{
						using (var stream = EnsureOpen(streamOpener, name))
						{
							material.Texture = Texture2D.FromStream(Nrs.GraphicsDevice, stream);
						}
					}
				}

				result.Materials.Add(material);
			}

			return result;
		}
	}
}
