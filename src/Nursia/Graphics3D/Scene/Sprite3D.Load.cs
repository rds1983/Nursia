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
		private class AttributeInfo
		{
			public int Size { get; private set; }
			public int ElementsCount { get; private set; }
			public VertexElementFormat Format { get; private set; }
			public VertexElementUsage Usage { get; private set; }

			public AttributeInfo(int size, int elementsCount, 
				VertexElementFormat format, VertexElementUsage usage)
			{
				Size = size;
				ElementsCount = elementsCount;
				Format = format;
				Usage = usage;
			}
		}

		private static readonly Dictionary<string, AttributeInfo> _attributes = new Dictionary<string, AttributeInfo>
		{
			["POSITION"] = new AttributeInfo(12, 3, VertexElementFormat.Vector3, VertexElementUsage.Position),
			["NORMAL"] = new AttributeInfo(12, 3, VertexElementFormat.Vector3, VertexElementUsage.Normal),
			["TEXCOORD"] = new AttributeInfo(8, 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate),
			["BLENDWEIGHT"] = new AttributeInfo(8, 2, VertexElementFormat.Vector2, VertexElementUsage.BlendWeight)
		};
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

		private static Matrix LoadTransform(JObject data)
		{
			var scale = Vector3.One;
			JToken token;
			if (data.TryGetValue("scale", out token))
			{
				scale = token.ToVector3();
			}

			var translation = Vector3.Zero;
			if (data.TryGetValue("translation", out token))
			{
				translation = token.ToVector3();
			}

			var rotation = Vector4.Zero;
			if (data.TryGetValue("rotation", out token))
			{
				rotation = token.ToVector4();
			}

			var quaternion = new Quaternion(rotation.X,
				rotation.Y, rotation.Z, rotation.W);

			/*			var result = Matrix.CreateScale(scale) *
							Matrix.CreateTranslation(translate) *
							Matrix.CreateFromQuaternion(quaternion);*/

			float xs = quaternion.X * 2f, ys = quaternion.Y * 2f, zs = quaternion.Z * 2f;
			float wx = quaternion.W * xs, wy = quaternion.W * ys, wz = quaternion.W * zs;
			float xx = quaternion.X * xs, xy = quaternion.X * ys, xz = quaternion.X * zs;
			float yy = quaternion.Y * ys, yz = quaternion.Y * zs, zz = quaternion.Z * zs;

			Matrix result;
			result.M11 = scale.X * (1.0f - (yy + zz));
			result.M12 = scale.Y * (xy - wz);
			result.M13 = scale.Z * (xz + wy);
			result.M14 = translation.X;

			result.M21 = scale.X * (xy + wz);
			result.M22 = scale.Y * (1.0f - (xx + zz));
			result.M23 = scale.Z * (yz - wx);
			result.M24 = translation.Y;

			result.M31 = scale.X * (xz - wy);
			result.M32 = scale.Y * (yz + wx);
			result.M33 = scale.Z * (1.0f - (xx + yy));
			result.M34 = translation.Z;

			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		private static Bone LoadBone(JObject data)
		{
			if (data == null)
			{
				return null;
			}

			var result = new Bone
			{
				Id = data.EnsureString("node"),
				Transform = Matrix.Invert(LoadTransform(data))
			};

			return result;
		}

		private static BoneNode LoadBoneNode(JObject data)
		{
			if (data == null)
			{
				return null;
			}

			var result = new BoneNode
			{
				Id = data.GetId(),
				Transform = LoadTransform(data)
			};

			if (data.ContainsKey("children"))
			{
				var children = (JArray)data["children"];
				foreach (JObject child in children)
				{
					var childNode = LoadBoneNode(child);
					childNode.Parent = result;
					result.Children.Add(childNode);
				}
			}

			return result;
		}

		private static VertexDeclaration LoadDeclaration(JArray data, out int elementsPerData)
		{
			elementsPerData = 0;
			var elements = new List<VertexElement>();
			var offset = 0;
			foreach(var elementData in data)
			{
				var name = elementData.ToString();
				var usage = 0;

				// Remove last digit
				var lastChar = name[name.Length - 1];
				if (char.IsDigit(lastChar))
				{
					name = name.Substring(0, name.Length - 1);
					usage = int.Parse(lastChar.ToString());
				}

				AttributeInfo attributeInfo;
				if (!_attributes.TryGetValue(name, out attributeInfo))
				{
					throw new Exception(string.Format("Unknown attribute '{0}'", name));
				}

				var element = new VertexElement(offset, 
					attributeInfo.Format, 
					attributeInfo.Usage, 
					usage);
				elements.Add(element);

				offset += attributeInfo.Size;
				elementsPerData += attributeInfo.ElementsCount;
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

			var result = new VertexBuffer(Nrs.GraphicsDevice, declaration, rowsCount, BufferUsage.None);
			result.SetData(byteData);

			return result;
		}

		public static Sprite3D LoadFromJson(string json, Func<string, Texture2D> textureGetter)
		{
			var root = JObject.Parse(json);

			var result = new Sprite3D();
			var meshesData = (JArray)root["meshes"];
			var meshes = new Dictionary<string, MeshPart>();
			foreach (JObject meshData in meshesData)
			{
				// Determine vertex type
				int elementsPerRow;
				var declaration = LoadDeclaration((JArray)meshData["attributes"], out elementsPerRow);
				var vertices = (JArray)meshData["vertices"];

				int bonesCount = 0;
				foreach(var element in declaration.GetVertexElements())
				{
					if (element.VertexElementUsage != VertexElementUsage.BlendWeight)
					{
						continue;
					}

					if (element.UsageIndex + 1 > bonesCount)
					{
						bonesCount = element.UsageIndex + 1;
					}
				}
				
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

				var partsData = (JArray)meshData["parts"];
				foreach (JObject partData in partsData)
				{
					var id = partData.GetId();

					// var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
					var indicesData = (JArray)partData["indices"];
					var indices = new short[indicesData.Count];
					for (var i = 0; i < indicesData.Count; ++i)
					{
						indices[i] = Convert.ToInt16(indicesData[i]);
					}

					var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits,
						indices.Length, BufferUsage.None);
					indexBuffer.SetData(indices);

					meshes[id] = new MeshPart
					{
						IndexBuffer = indexBuffer,
						VertexBuffer = vertexBuffer,
						BonesPerMesh = bonesPerMesh
					};
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

				var texturesData = (JArray)materialData["textures"];
				var name = texturesData[0]["filename"].ToString();
				if (!string.IsNullOrEmpty(name))
				{
					material.Texture = textureGetter(name);
				}

				result.Materials.Add(material);
			}

			// Load nodes hierarchy
			var nodes = (JObject)((JArray)root["nodes"])[0];
			var nodesData = (JArray)nodes["children"];
			foreach(JObject childData in nodesData)
			{
				if (childData.ContainsKey("children"))
				{
					// Bone
					result.RootNode = LoadBoneNode(nodes);
				} else
				{
					// Mesh
					var mesh = new Mesh
					{
						Id = childData.GetId()
					};

					var partsData = (JArray)childData["parts"];
					foreach(JObject partData in partsData)
					{
						var meshPart = meshes[partData["meshpartid"].ToString()];
						var newPart = new MeshPart
						{
							IndexBuffer = meshPart.IndexBuffer,
							VertexBuffer = meshPart.VertexBuffer,
							BonesPerMesh = meshPart.BonesPerMesh
						};

						var materialId = partData["materialid"].ToString();
						newPart.Material = (from m in result.Materials where m.Id == materialId select m).First();

						if (partData.ContainsKey("bones"))
						{
							var bonesData = (JArray)partData["bones"];
							foreach (JObject boneData in bonesData)
							{
								var bone = LoadBone(boneData);
								newPart.Bones.Add(bone);
							}
						}

						mesh.Parts.Add(newPart);
					}

					result.Meshes.Add(mesh);
				}
			}

			if (result.RootNode != null)
			{
				var boneNodesDict = new Dictionary<string, BoneNode>();
				result.TraverseBoneNodes(bn =>
				{
					boneNodesDict[bn.Id] = bn;
				});

				// Set parent nodes
				foreach (var m in result.Meshes)
				{
					foreach (var part in m.Parts)
					{
						foreach (var bone in part.Bones)
						{
							BoneNode bn;
							if (boneNodesDict.TryGetValue(bone.Id, out bn))
							{
								bone.ParentNode = bn;
							}
						}
					}
				}
			}

			return result;
		}
	}
}
