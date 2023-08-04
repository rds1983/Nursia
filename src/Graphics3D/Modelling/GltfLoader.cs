using System;
using System.Collections.Generic;
using glTFLoader;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.Modelling
{
	internal class GltfLoader
	{
		private static Random rnd = new Random();

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
					var vertexBufferBindings = new List<VertexBufferBinding>();
					var boundingSphere = new BoundingSphere();
					foreach (var pair in primitive.Attributes)
					{
						var accessor = _gltf.Accessors[pair.Value];
						var element = new VertexElement();

						switch (pair.Key)
						{
							case "POSITION":
								element.VertexElementUsage = VertexElementUsage.Position;
								break;
							case "NORMAL":
								element.VertexElementUsage = VertexElementUsage.Normal;
								break;
							case "TEXCOORD_0":
								element.VertexElementUsage = VertexElementUsage.TextureCoordinate;
								break;
							case "TANGENT":
								element.VertexElementUsage = VertexElementUsage.Tangent;
								break;
							case "JOINTS_0":
							case "WEIGHTS_0":
								// TODO: Check if that could be supported
								continue;
							default:
								throw new NotSupportedException($"Attribute {pair.Key} isn't supported");
						}

						element.VertexElementFormat = GetAccessorFormat(pair.Value);

						var vd = new VertexDeclaration(element);
						var vertexBuffer = new VertexBuffer(Nrs.GraphicsDevice, vd, accessor.Count, BufferUsage.None);

						var data = GetAccessorData(pair.Value);
						vertexBuffer.SetData(data.Array, data.Offset, data.Count);

						if (element.VertexElementUsage == VertexElementUsage.Position)
						{
							var partPositions = new Vector3[data.Count / 12];

							unsafe
							{
								fixed (byte* bptr = &data.Array[data.Offset])
								fixed (Vector3* vptr = partPositions)
								{
									System.Buffer.MemoryCopy(bptr, vptr, partPositions.Length * sizeof(Vector3), data.Count);
								}
							}
							boundingSphere = BoundingSphere.CreateFromPoints(partPositions);
						}

						var vbb = new VertexBufferBinding(vertexBuffer);
						vertexBufferBindings.Add(vbb);
					}

					// Determine vertex count
					int? vertexCount = null;
					foreach (var vbb in vertexBufferBindings)
					{
						if (vertexCount == null)
						{
							vertexCount = vbb.VertexBuffer.VertexCount;
						}
						else if (vertexCount.Value != vbb.VertexBuffer.VertexCount)
						{
							throw new NotSupportedException("Vertex buffers of different sizes aren't supported");
						}
					}

					if (vertexCount == null)
					{
						throw new NotSupportedException("Absence of vertex buffers isn't supported");
					}

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

					var mesh = new Mesh
					{
						VertexBuffers = vertexBufferBindings.ToArray(),
						IndexBuffer = indexBuffer,
					};

					var material = new Material
					{
						DiffuseColor = new Color((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256)),
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
			foreach (var mesh in meshes)
			{
				result.Meshes.Add(mesh);
			}

			return result;
		}
	}
}
