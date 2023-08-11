using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using glTFLoader;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Nursia.Utilities;
using static glTFLoader.Schema.AnimationChannelTarget;

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

		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct VertexPositionNormalTextureSkin
		{
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 Texture;
			public byte i1, i2, i3, i4;
			public float w1, w2, w3, w4;
		}

		struct PathInfo
		{
			public int Sampler;
			public PathEnum Path;

			public PathInfo(int sampler, PathEnum path)
			{
				Sampler = sampler;
				Path = path;
			}
		}

		class Transform
		{
			public Vector3 Translation;
			public Quaternion Rotation;
			public Vector3 Scale;

			public Transform()
			{
				Translation = Vector3.Zero;
				Rotation = Quaternion.Identity;
				Scale = Vector3.One;
			}

			public Matrix ToMatrix() => CreateTransform(Translation, Scale, Rotation);
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

		private T[] GetAccessorAs<T>(int accessorIndex)
		{
			var type = typeof(T);
			if (type != typeof(float) && type != typeof(Vector3) && type != typeof(Vector4) && type != typeof(Quaternion))
			{
				throw new NotSupportedException("Only float/Vector3/Vector4 types are supported");
			}

			var accessor = _gltf.Accessors[accessorIndex];
			if (accessor.Type == Accessor.TypeEnum.SCALAR && type != typeof(float))
			{
				throw new NotSupportedException("Scalar type could be converted only to float");
			}

			if (accessor.Type == Accessor.TypeEnum.VEC3 && type != typeof(Vector3))
			{
				throw new NotSupportedException("VEC3 type could be converted only to Vector3");
			}

			if (accessor.Type == Accessor.TypeEnum.VEC4 && type != typeof(Vector4) && type != typeof(Quaternion))
			{
				throw new NotSupportedException("VEC4 type could be converted only to Vector4 or Quaternion");
			}

			var bytes = GetAccessorData(accessorIndex);

			var count = bytes.Count / Marshal.SizeOf(typeof(T));
			var result = new T[count];

			GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
			try
			{
				IntPtr pointer = handle.AddrOfPinnedObject();
				Marshal.Copy(bytes.Array, bytes.Offset, pointer, bytes.Count);
			}
			finally
			{
				if (handle.IsAllocated)
				{
					handle.Free();
				}
			}

			return result;
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

		private static Matrix CreateTransform(Vector3 translation, Vector3 scale, Quaternion rotation)
		{
			return Matrix.CreateFromQuaternion(rotation) *
				Matrix.CreateScale(scale) *
				Matrix.CreateTranslation(translation);
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

			return CreateTransform(translation, scale, quaternion);
		}

		private static ModelNode ToModelNode(Node gltfNode)
		{
			var result = new ModelNode
			{
				Id = gltfNode.Name
			};

			var scale = gltfNode.Scale != null ? gltfNode.Scale.ToVector3() : Vector3.One;
			var translation = gltfNode.Translation != null ? gltfNode.Translation.ToVector3() : Vector3.Zero;
			var rotation = gltfNode.Rotation != null ? gltfNode.Rotation.ToQuaternion() : new Quaternion();

			result.Transform = CreateTransform(translation, scale, rotation);

			return result;
		}

		private static Transform EnsureTransform(IDictionary<float, Transform> timeline, float time)
		{
			if (!timeline.TryGetValue(time, out Transform result))
			{
				result = new Transform();
				timeline[time] = result;
			}

			return result;
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
								element.Usage = VertexElementUsage.BlendIndices;
								break;
							case "WEIGHTS_0":
								element.Usage = VertexElementUsage.BlendWeight;
								break;
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

					/* var vertices = new VertexPositionNormalTexture[vertexCount.Value];
					vertexBuffer.GetData(vertices); */

					if (primitive.Indices == null)
					{
						throw new NotSupportedException("Meshes without indices arent supported");
					}

					var indexAccessor = _gltf.Accessors[primitive.Indices.Value];
					if (indexAccessor.Type != Accessor.TypeEnum.SCALAR || indexAccessor.ComponentType != Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
					{
						throw new NotSupportedException("Only scalar/unsigned short index buffer are supported");
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

			// Load all nodes
			var allNodes = new List<ModelNode>();
			for(var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];
				allNodes.Add(ToModelNode(gltfNode));
			}

			// Set children
			for (var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];
				var modelNode = allNodes[i];

				if (gltfNode.Children != null)
				{
					foreach(var childIndex  in gltfNode.Children)
					{
						modelNode.Children.Add(allNodes[childIndex]);
					}
				}
			}

			var rootNodexIndex = _gltf.Scenes[_gltf.Scene.Value].Nodes[0];
			var result = new NursiaModel
			{
				RootNode = allNodes[rootNodexIndex]
			};

			foreach (var meshNode in meshes)
			{
				result.Meshes.Add(meshNode);
			}

			if (_gltf.Animations != null)
			{
				foreach (var gltfAnimation in _gltf.Animations)
				{
					var animation = new ModelAnimation
					{
						Id = gltfAnimation.Name
					};

					var channelsDict = new Dictionary<int, List<PathInfo>>();
					foreach (var channel in gltfAnimation.Channels)
					{
						if (!channelsDict.TryGetValue(channel.Target.Node.Value, out List<PathInfo> targets))
						{
							targets = new List<PathInfo>();
							channelsDict[channel.Target.Node.Value] = targets;
						}

						targets.Add(new PathInfo(channel.Sampler, channel.Target.Path));
					}

					foreach (var pair in channelsDict)
					{
						var nodeAnimation = new NodeAnimation
						{
							Node = allNodes[pair.Key]
						};

						var timeline = new SortedDictionary<float, Transform>();
						foreach (var pathInfo in pair.Value)
						{
							var sampler = gltfAnimation.Samplers[pathInfo.Sampler];
							var times = GetAccessorAs<float>(sampler.Input);

							switch (pathInfo.Path)
							{
								case PathEnum.translation:
									var translations = GetAccessorAs<Vector3>(sampler.Output);
									if (times.Length != translations.Length)
									{
										throw new NotSupportedException("Translation length is different from times length");
									}

									for (var i = 0; i < times.Length; ++i)
									{
										EnsureTransform(timeline, times[i]).Translation = translations[i];
									}

									break;
								case PathEnum.rotation:
									var rotations = GetAccessorAs<Quaternion>(sampler.Output);
									if (times.Length != rotations.Length)
									{
										throw new NotSupportedException("Rotations length is different from times length");
									}

									for (var i = 0; i < times.Length; ++i)
									{
										EnsureTransform(timeline, times[i]).Rotation = rotations[i];
									}

									break;
								case PathEnum.scale:
									var scales = GetAccessorAs<Vector3>(sampler.Output);
									if (times.Length != scales.Length)
									{
										throw new NotSupportedException("Scales length is different from times length");
									}

									for (var i = 0; i < times.Length; ++i)
									{
										EnsureTransform(timeline, times[i]).Scale = scales[i];
									}

									break;
								case PathEnum.weights:
									break;
							}
						}

						foreach (var pair2 in timeline)
						{
							nodeAnimation.Frames.Add(new AnimationKeyframe
							{
								Time = TimeSpan.FromSeconds(pair2.Key),
								Transform = pair2.Value.ToMatrix()
							});
						}

						animation.BoneAnimations.Add(nodeAnimation);
					}

					result.Animations[animation.Id] = animation;
				}
			}

			return result;
		}
	}
}