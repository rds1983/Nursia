using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetManagementBase;
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

		private AssetManager _assetManager;
		private string _assetName;
		private Gltf _gltf;
		private readonly Dictionary<int, byte[]> _bufferCache = new Dictionary<int, byte[]>();

		private byte[] GetBuffer(int index)
		{
			byte[] result;
			if (_bufferCache.TryGetValue(index, out result))
			{
				return result;
			}

			result = _gltf.LoadBinaryBuffer(index, path =>
			{
				if (string.IsNullOrEmpty(path))
				{
					path = _assetName;
				}

				using (var stream = _assetManager.OpenAssetStream(path))
				{
					return Interface.LoadBinaryBuffer(stream);
				}
			});
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
			if (type != typeof(float) && type != typeof(Vector3) && type != typeof(Vector4) && type != typeof(Quaternion) && type != typeof(Matrix))
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

			if (accessor.Type == Accessor.TypeEnum.MAT4 && type != typeof(Matrix))
			{
				throw new NotSupportedException("MAT4 type could be converted only to Matrix");
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

		private void LoadAnimationTransforms<T>(AnimationTransforms<T> animationTransforms, float[] times, AnimationSampler sampler)
		{
			var translations = GetAccessorAs<T>(sampler.Output);
			if (times.Length != translations.Length)
			{
				throw new NotSupportedException("Translation length is different from times length");
			}

			for (var i = 0; i < times.Length; ++i)
			{
				animationTransforms.Values.Add(new AnimationTransformKeyframe<T>(times[i], translations[i]));
			}

			animationTransforms.Interpolation = sampler.Interpolation;
		}

		public NursiaModel Load(AssetManager manager, string assetName)
		{
			_assetManager = manager;
			_assetName = assetName;
			using (var stream = manager.OpenAssetStream(assetName))
			{
				_gltf = Interface.LoadModel(stream);
			}

			var meshes = new List<List<MeshPart>>();
			foreach (var gltfMesh in _gltf.Meshes)
			{
				var meshParts = new List<MeshPart>();
				foreach (var primitive in gltfMesh.Primitives)
				{
					// Read vertex declaration
					var vertexInfos = new List<VertexElementInfo>();
					int? vertexCount = null;
					var hasSkin = false;
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
								hasSkin = true;
								element.Usage = VertexElementUsage.BlendIndices;
								break;
							case "WEIGHTS_0":
								hasSkin = true;
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
						DiffuseColor = Color.White,
						Texture = Assets.White
					};

					var meshPart = new MeshPart
					{
						Mesh = mesh,
						BoundingSphere = boundingSphere,
						Material = material
					};

					if (hasSkin)
					{
						//						meshPart.BonesPerMesh = BonesPerMesh.Four;
					}

					meshParts.Add(meshPart);
				}

				meshes.Add(meshParts);
			}

			var result = new NursiaModel();

			// Load all nodes
			var allNodes = new List<ModelNode>();
			for (var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];

				var modelNode = new ModelNode
				{
					Id = gltfNode.Name,
					DefaultTranslation = gltfNode.Translation != null ? gltfNode.Translation.ToVector3() : Vector3.Zero,
					DefaultScale = gltfNode.Scale != null ? gltfNode.Scale.ToVector3() : Vector3.One,
					DefaultRotation = gltfNode.Rotation != null ? gltfNode.Rotation.ToQuaternion() : Quaternion.Identity
				};

				if (gltfNode.Mesh != null)
				{
					modelNode.Parts.AddRange(meshes[gltfNode.Mesh.Value]);
				}

				if (gltfNode.Skin != null)
				{
					var gltfSkin = _gltf.Skins[gltfNode.Skin.Value];
					var skin = new Skin();
					foreach (var jointIndex in gltfSkin.Joints)
					{
						skin.Joints.Add(allNodes[jointIndex]);
					}

					skin.Transforms = GetAccessorAs<Matrix>(gltfSkin.InverseBindMatrices.Value);
					modelNode.Skin = skin;
				}

				allNodes.Add(modelNode);
			}

			// Set children
			for (var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];
				var modelNode = allNodes[i];

				if (gltfNode.Children != null)
				{
					foreach (var childIndex in gltfNode.Children)
					{
						allNodes[childIndex].Parent = modelNode;
						modelNode.Children.Add(allNodes[childIndex]);
					}
				}
			}

			var scene = _gltf.Scenes[_gltf.Scene.Value];
			foreach (var node in scene.Nodes)
			{
				result.Meshes.Add(allNodes[node]);
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
						var nodeAnimation = new NodeAnimation(allNodes[pair.Key]);

						foreach (var pathInfo in pair.Value)
						{
							var sampler = gltfAnimation.Samplers[pathInfo.Sampler];
							var times = GetAccessorAs<float>(sampler.Input);

							switch (pathInfo.Path)
							{
								case PathEnum.translation:
									LoadAnimationTransforms(nodeAnimation.Translations, times, sampler);
									break;
								case PathEnum.rotation:
									LoadAnimationTransforms(nodeAnimation.Rotations, times, sampler);
									break;
								case PathEnum.scale:
									LoadAnimationTransforms(nodeAnimation.Scales, times, sampler);
									break;
								case PathEnum.weights:
									break;
							}
						}

						animation.BoneAnimations.Add(nodeAnimation);
					}

					animation.UpdateStartEnd();
					result.Animations[animation.Id] = animation;
				}
			}

			result.ResetTransforms();

			return result;
		}
	}
}