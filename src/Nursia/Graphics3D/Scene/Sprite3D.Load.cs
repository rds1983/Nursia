using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nursia.Graphics3D.Scene
{
	partial class Sprite3D
	{
		internal const string IdName = "name";

		private static Stream EnsureOpen(Func<string, Stream> streamOpener, string name)
		{
			var result = streamOpener(name);
			if (result == null)
			{
				throw new Exception(string.Format("stream is null for name '{0}'", name));
			}

			return result;
		}

		private static Bone LoadBone(JObject data)
		{
			if (data == null)
			{
				return null;
			}

			var scale = data["scale"].ToVector3();
			var translate = data["translate"].ToVector3();
			var rotation = data["rotation"].ToVector4();

			var quaternion = new Quaternion(rotation.X, 
				rotation.Y, rotation.Z, rotation.W);

			var transform = Matrix.CreateScale(scale) *
				Matrix.CreateTranslation(translate) *
				Matrix.CreateFromQuaternion(quaternion);

			var result = new Bone
			{
				Id = data.GetId(),
				Transform = transform
			};

			if (data.ContainsKey("children"))
			{
				var children = (JArray)data["children"];
				foreach (JObject child in children)
				{
					var childNode = LoadBone(child);
					childNode.Parent = result;
					result.Children.Add(childNode);
				}
			}

			return result;
		}

		private static VertexDeclaration LoadDeclaration(JArray data)
		{
			var elements = new List<VertexElement>();
			foreach(JObject elementData in data)
			{
				var offset = int.Parse(elementData["offset"].ToString());
				var format = elementData["format"].ParseEnum<VertexElementFormat>();
				var usage = elementData["usage"].ParseEnum<VertexElementUsage>();

				var element = new VertexElement(offset, format, usage, 0);
				elements.Add(element);
			}

			return new VertexDeclaration(elements.ToArray());
		}

		private static void LoadFloat(byte[] dest, ref int destIdx, float data)
		{
			var byteData = BitConverter.GetBytes(data);
			Array.Copy(byteData, 0, dest, destIdx, byteData.Length);
			destIdx += byteData.Length;
		}

		private static void LoadByte(byte[] dest, ref int destIdx, int data)
		{
			if (data > byte.MaxValue)
			{
				throw new Exception(string.Format("Only byte bone indices suported. {0}", data));
			}

			dest[destIdx] = (byte)data;
			++destIdx;
		}

		private static VertexBuffer LoadVertexBuffer(
			VertexDeclaration declaration, 
			int elementsPerRow,
			JArray data)
		{
			var rowsCount = data.Count / elementsPerRow;
			var byteData = new byte[rowsCount * declaration.VertexStride];

			var destIdx = 0;
			var srcIdx = 0;
			for (var i = 0; i < rowsCount; ++i)
			{
				var elements = declaration.GetVertexElements();
				for (var j = 0; j < elements.Length; ++j)
				{
					var element = elements[j];

					switch (element.VertexElementFormat)
					{
						case VertexElementFormat.Vector2:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						case VertexElementFormat.Vector3:
						case VertexElementFormat.Color:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						case VertexElementFormat.Vector4:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						case VertexElementFormat.Byte4:
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							break;
						default:
							throw new Exception(string.Format("{0} not supported", element.VertexElementFormat));
					}
				}
			}



			var result = new VertexBuffer(Nrs.GraphicsDevice, declaration, data.Count, BufferUsage.None);
			result.SetData(byteData);

			return result;
		}

		public static Sprite3D LoadFromJson(string json, Func<string, Texture2D> textureGetter)
		{
			var root = JObject.Parse(json);

			var result = new Sprite3D();
			var meshesData = (JArray)root["meshes"];
			var meshes = new Dictionary<string, List<MeshPart>>();
			foreach (JObject meshData in meshesData)
			{
				var id = meshData.EnsureString("meshNodeName");

				// Determine vertex type
				var declaration = LoadDeclaration((JArray)meshData["declaration"]);
				var elementsPerRow = int.Parse(meshData["elementsPerRow"].ToString());
				var vertices = (JArray)meshData["vertices"];

				var bonesCount = (int)meshData["bonesCount"];

				var bonesPerMesh = BonesPerMesh.None;
				if (bonesCount >= 3)
				{
					bonesPerMesh = BonesPerMesh.Four;
				}
				else if (bonesCount == 2)
				{
					bonesPerMesh = BonesPerMesh.Two;
				}
				else if (bonesCount == 1)
				{
					bonesPerMesh = BonesPerMesh.One;
				}

				var vertexBuffer = LoadVertexBuffer(declaration, elementsPerRow, vertices);

				// var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
				var indicesData = (JArray)meshData["indices"];
				var indices = new short[indicesData.Count];
				for (var i = 0; i < indicesData.Count; ++i)
				{
					indices[i] = Convert.ToInt16(indicesData[i]);
				}

				var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits,
					indices.Length, BufferUsage.None);
				indexBuffer.SetData(indices);

				List<MeshPart> m;
				if (!meshes.TryGetValue(id, out m))
				{
					m = new List<MeshPart>();
					meshes[id] = m;
				}

				var part = new MeshPart
				{
					IndexBuffer = indexBuffer,
					VertexBuffer = vertexBuffer,
					MaterialName = meshData["material"].ToString(),
					BonesPerMesh = bonesPerMesh
				};

				if (meshData.ContainsKey("bones"))
				{
					var bones = (JArray)meshData["bones"];
					foreach (JObject boneData in bones)
					{
						var bone = LoadBone(boneData);
						part.Bones.Add(bone);
					}
				}

				m.Add(part);
			}

			// Add meshes
			foreach(var pair in meshes)
			{
				var mesh = new Mesh
				{
					Id = pair.Key
				};

				mesh.Parts.AddRange(pair.Value);
				result.Meshes.Add(mesh);
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
			foreach (var m in result.Meshes)
			{
				foreach (var part in m.Parts)
				{
					part.Material = (from mt in result.Materials where mt.Id == part.MaterialName select mt).First();
				}
			}

			return result;
		}
	}
}
