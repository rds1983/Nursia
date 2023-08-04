using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using glTFLoader;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia.Graphics3D.Modelling
{
	internal class GltfLoader
	{
		struct VertexElementInfo
		{
			public VertexElementFormat Format;
			public VertexElementUsage Usage;
			public int AccessorIndex;

			public VertexElementInfo(VertexElementFormat format, VertexElementUsage usage, int accessorIndex)
			{
				Format = format;
				Usage = usage;
				AccessorIndex = accessorIndex;
			}
		}

		private string _path;
		private Gltf _gltf;
		private readonly Dictionary<int, byte[]> _bufferCache = new Dictionary<int, byte[]>();

		private byte[] GetBuffer(int index)
		{
			byte[] result;
			if (_bufferCache.TryGetValue(index, out result))
			{
				return result;
			}

			result = _gltf.LoadBinaryBuffer(index, _path);
			_bufferCache[index] = result;

			return result;
		}

		private ArraySegment<byte> GetAccessorData(int accessorIndex)
		{
			var accessor = _gltf.Accessors[accessorIndex];
			if (accessor.BufferView == null)
			{
				throw new NotSupportedException("Accessors without buffer index arent supported");
			}

			var bufferView = _gltf.BufferViews[accessor.BufferView.Value];
			var buffer = GetBuffer(bufferView.Buffer);

			return new ArraySegment<byte>(buffer, bufferView.ByteOffset, bufferView.ByteLength);
		}

		private VertexElementFormat GetAccessorFormat(int index)
		{
			var accessor = _gltf.Accessors[index];

			switch (accessor.Type)
			{
				case Accessor.TypeEnum.VEC2:
					if (accessor.ComponentType == Accessor.ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector2;
					}
					break;
				case Accessor.TypeEnum.VEC3:
					if (accessor.ComponentType == Accessor.ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector3;
					}
					break;
				case Accessor.TypeEnum.VEC4:
					if (accessor.ComponentType == Accessor.ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector4;
					}
					else if (accessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_BYTE)
					{
						return VertexElementFormat.Byte4;
					}
					break;
			}

			throw new NotSupportedException($"Accessor of type {accessor.Type} and component type {accessor.ComponentType} isn't supported");
		}

		public NursiaModel Load(string path)
		{
			_path = path;
			_gltf = Interface.LoadModel(path);

			var meshes = new List<MeshNode>();
			foreach (var gltfMesh in _gltf.Meshes)
			{
				var meshNode = new MeshNode
				{
					Id = gltfMesh.Name
				};

				foreach (var primitive in gltfMesh.Primitives)
				{
					// Read vertex declaration
					var vertexInfos = new List<VertexElementInfo>();
					int? vertexCount = null;
					foreach (var pair in primitive.Attributes)
					{
						var accessor = _gltf.Accessors[pair.Value];
						var newVertexCount = accessor.Count;
						if (vertexCount != null && vertexCount.Value != newVertexCount)
						{
							throw new NotSupportedException($"Vertex count changed. Previous value: {vertexCount}. New value: {newVertexCount}");
						}

						vertexCount = newVertexCount;

						var element = new VertexElementInfo();
						switch (pair.Key)
						{
							case "POSITION":
								element.Usage = VertexElementUsage.Position;
								break;
							case "NORMAL":
								element.Usage = VertexElementUsage.Normal;
								break;
							case "TEXCOORD_0":
								element.Usage = VertexElementUsage.TextureCoordinate;
								break;
							case "TANGENT":
								element.Usage = VertexElementUsage.Tangent;
								break;
							case "JOINTS_0":
							case "WEIGHTS_0":
								// TODO: Check if that could be supported
								continue;
							default:
								throw new NotSupportedException($"Attribute {pair.Key} isn't supported");
						}

						element.Format = GetAccessorFormat(pair.Value);
						element.AccessorIndex = pair.Value;

						vertexInfos.Add(element);
					}

					if (vertexCount == null)
					{
						throw new NotSupportedException("Vertex count is not set");
					}

					var vertexElements = new VertexElement[vertexInfos.Count];
					var offset = 0;
					for (var i = 0; i < vertexInfos.Count; ++i)
					{
						vertexElements[i] = new VertexElement(offset, vertexInfos[i].Format, vertexInfos[i].Usage, 0);
						offset += vertexInfos[i].Format.GetSize();
					}

					var vd = new VertexDeclaration(vertexElements);
					var vertexBuffer = new VertexBuffer(Nrs.GraphicsDevice, vd, vertexCount.Value, BufferUsage.None);

					// Set vertex data
					var vertexData = new byte[vertexCount.Value * vd.VertexStride];
					var partPositions = new List<Vector3>();
					offset = 0;
					for (var i = 0; i < vertexInfos.Count; ++i)
					{
						var sz = vertexInfos[i].Format.GetSize();
						var data = GetAccessorData(vertexInfos[i].AccessorIndex);
						for (var j = 0; j < vertexCount.Value; ++j)
						{
							Array.Copy(data.Array, data.Offset + j * sz, vertexData, j * vd.VertexStride + offset, sz);

							if (vertexInfos[i].Usage == VertexElementUsage.Position)
							{
								unsafe
								{
									fixed (byte* bptr = &data.Array[data.Offset + j * sz])
									{
										Vector3* vptr = (Vector3*)bptr;
										partPositions.Add(*vptr);
									}
								}
							}
						}

						offset += sz;
					}

					vertexBuffer.SetData(vertexData);
					var boundingSphere = BoundingSphere.CreateFromPoints(partPositions);

					/*					var vertices = new VertexPositionNormalTexture[vertexCount.Value];
										vertexBuffer.GetData(vertices);*/

					if (primitive.Indices == null)
					{
						throw new NotSupportedException("Meshes without indices arent supported");
					}

					var indexAccessor = _gltf.Accessors[primitive.Indices.Value];
					if (indexAccessor.Type != Accessor.TypeEnum.SCALAR || indexAccessor.ComponentType != Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
					{
						throw new NotSupportedException("Only Scalar/unsigned short index buffer are supported");
					}

					var indexData = GetAccessorData(primitive.Indices.Value);
					var indexBuffer = new IndexBuffer(Nrs.GraphicsDevice, IndexElementSize.SixteenBits, indexAccessor.Count, BufferUsage.None);
					indexBuffer.SetData(0, indexData.Array, indexData.Offset, indexData.Count);

					/*					var indices = new ushort[indexAccessor.Count];
										indexBuffer.GetData(indices);*/

					var mesh = new Mesh
					{
						VertexBuffer = vertexBuffer,
						IndexBuffer = indexBuffer,
					};

					var material = new Material
					{
						DiffuseColor = Color.GreenYellow,
						Texture = Assets.White
					};

					var meshPart = new MeshPart
					{
						Mesh = mesh,
						BoundingSphere = boundingSphere,
						Material = material
					};

					meshNode.Parts.Add(meshPart);
				}

				meshes.Add(meshNode);
			}

			var result = new NursiaModel();

			foreach (var meshNode in meshes)
			{
				result.Meshes.Add(meshNode);
			}

			return result;
		}
	}
}
