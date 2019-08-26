// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Assimp;
using Assimp.Unmanaged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;
using Nursia.ModelImporter.Content;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace Nursia.ModelImporter
{
	class Importer
	{
		public static readonly string[] SupportedExtensions =
		{
		".dae", // Collada
        ".gltf", "glb", // glTF
        ".blend", // Blender 3D
        ".3ds", // 3ds Max 3DS
        ".ase", // 3ds Max ASE
        ".obj", // Wavefront Object
        ".ifc", // Industry Foundation Classes (IFC/Step)
        ".xgl", ".zgl", // XGL
        ".ply", // Stanford Polygon Library
        ".dxf", // AutoCAD DXF
        ".lwo", // LightWave
        ".lws", // LightWave Scene
        ".lxo", // Modo
        ".stl", // Stereolithography
        ".ac", // AC3D
        ".ms3d", // Milkshape 3D
        ".cob", ".scn", // TrueSpace
        ".bvh", // Biovision BVH
        ".csm", // CharacterStudio Motion
        ".irrmesh", // Irrlicht Mesh
        ".irr", // Irrlicht Scene
        ".mdl", // Quake I, 3D GameStudio (3DGS)
        ".md2", // Quake II
        ".md3", // Quake III Mesh
        ".pk3", // Quake III Map/BSP
        ".mdc", // Return to Castle Wolfenstein
        ".md5", // Doom 3
        ".smd", ".vta", // Valve Model 
        ".ogex", // Open Game Engine Exchange
        ".3d", // Unreal
        ".b3d", // BlitzBasic 3D
        ".q3d", ".q3s", // Quick3D
        ".nff", // Neutral File Format, Sense8 WorldToolKit
        ".off", // Object File Format
        ".ter", // Terragen Terrain
        ".hmp", // 3D GameStudio (3DGS) Terrain
        ".ndo", // Izware Nendo
		};

		// Assimp has a few limitations (not all FBX files are supported):
		// FBX files reference objects using IDs. Therefore, it is possible to resolve
		// bones even if multiple bones/nodes have the same name. But Assimp references
		// bones only by name!
		// --> Limitation #1: A model cannot have more than one skeleton!
		// --> Limitation #2: Bone names need to be unique!
		//
		// Bones are represented by regular nodes, but there is no flag indicating whether
		// a node is a bone. A mesh in Assimp references deformation bones (= bones that
		// affect vertices) by name. That means, we can identify the nodes that represent
		// deformation bones. But there is no way to identify helper bones (= bones that 
		// belong to the skeleton, but do not affect vertices). As described in 
		// http://assimp.sourceforge.net/lib_html/data.html and 
		// http://gamedev.stackexchange.com/questions/26382/i-cant-figure-out-how-to-animate-my-loaded-model-with-assimp/26442#26442
		// we can only guess which nodes belong to a skeleton:
		// --> Limitation #3: The skeleton needs to be a direct child of the root node or
		//                    the mesh node!
		//
		// Node.Transform is irrelevant for bones. This transform is just the pose of the
		// bone at the time of the export. This could be one of the animation frames. It
		// is not necessarily the bind pose (rest pose)! For example, XNA's Dude.fbx does
		// NOT store the skeleton in bind pose.
		// The correct transform is stored in Mesh.Bones[i].OffsetMatrix. However, this
		// information is only available for deformation bones, not for helper bones.
		// --> Limitation #4: The skeleton either must not contain helper bones, or it must
		//                    be guaranteed that the skeleton is exported in bind pose!
		//
		// An FBX file does not directly store all animation values. In some FBX scene it
		// is insufficient to simply read the animation data from the file. Instead, the
		// animation properties of all relevant objects in the scene need to be evaluated.
		// For example, some animations are the result of the current skeleton pose + the
		// current animation. The current skeleton pose is not imported/processed by XNA.
		// Assimp does not include an "animation evaluater" that automatically bakes these
		// animations.
		// --> Limitation #5: All bones included in an animation need to be key framed.
		//                    (There is no automatic evaluation.)
		//
		// In FBX it is possible to define animations curves for some transform components
		// (e.g. translation X and Y) and leave other components (e.g. translation Z) undefined.
		// Assimp does not pick the right defaults for undefined components.
		// --> Limitation #6: When scale, rotation, or translation is animated, all components
		//                    X, Y, Z need to be key framed.

		#region Nested Types
		/// <summary>Defines the frame for local scale/rotation/translation of FBX nodes.</summary>
		/// <remarks>
		/// <para>
		/// The transformation pivot defines the frame for local scale/rotation/translation. The
		/// local transform of a node is:
		/// </para>
		/// <para>
		/// Local Transform = Translation * RotationOffset * RotationPivot * PreRotation
		///                   * Rotation * PostRotation * RotationPivotInverse * ScalingOffset
		///                   * ScalingPivot * Scaling * ScalingPivotInverse
		/// </para>
		/// <para>
		/// where the matrix multiplication order is right-to-left.
		/// </para>
		/// <para>
		/// 3ds max uses three additional transformations:
		/// </para>
		/// <para>
		/// Local Transform = Translation * Rotation * Scaling
		///                   * GeometricTranslation * GeometricRotation * GeometricScaling
		/// </para>
		/// <para>
		/// Transformation pivots are stored per FBX node. When Assimp hits an FBX node with
		/// a transformation pivot it generates additional nodes named
		/// </para>
		/// <para>
		///   <i>OriginalName</i>_$AssimpFbx$_<i>TransformName</i>
		/// </para>
		/// <para>
		/// where <i>TransformName</i> is one of: 
		/// </para>
		/// <para>
		///   Translation, RotationOffset, RotationPivot, PreRotation, Rotation, PostRotation,
		///   RotationPivotInverse, ScalingOffset, ScalingPivot, Scaling, ScalingPivotInverse,
		///   GeometricTranslation, GeometricRotation, GeometricScaling
		/// </para>
		/// </remarks>
		/// <seealso href="http://download.autodesk.com/us/fbx/20112/FBX_SDK_HELP/index.html?url=WS1a9193826455f5ff1f92379812724681e696651.htm,topicNumber=d0e7429"/>
		/// <seealso href="http://area.autodesk.com/forum/autodesk-fbx/fbx-sdk/the-makeup-of-the-local-matrix-of-an-kfbxnode/"/>
		private class FbxPivot
		{
			public static readonly FbxPivot Default = new FbxPivot();

			public Matrix? Translation;
			public Matrix? RotationOffset;
			public Matrix? RotationPivot;
			public Matrix? PreRotation;
			public Matrix? Rotation;
			public Matrix? PostRotation;
			public Matrix? RotationPivotInverse;
			public Matrix? ScalingOffset;
			public Matrix? ScalingPivot;
			public Matrix? Scaling;
			public Matrix? ScalingPivotInverse;
			public Matrix? GeometricTranslation;
			public Matrix? GeometricRotation;
			public Matrix? GeometricScaling;

			public Matrix GetTransform(Vector3? scale, Quaternion? rotation, Vector3? translation)
			{
				var transform = Matrix.Identity;

				if (GeometricScaling.HasValue)
					transform *= GeometricScaling.Value;
				if (GeometricRotation.HasValue)
					transform *= GeometricRotation.Value;
				if (GeometricTranslation.HasValue)
					transform *= GeometricTranslation.Value;
				if (ScalingPivotInverse.HasValue)
					transform *= ScalingPivotInverse.Value;
				if (scale.HasValue)
					transform *= Matrix.CreateScale(scale.Value);
				else if (Scaling.HasValue)
					transform *= Scaling.Value;
				if (ScalingPivot.HasValue)
					transform *= ScalingPivot.Value;
				if (ScalingOffset.HasValue)
					transform *= ScalingOffset.Value;
				if (RotationPivotInverse.HasValue)
					transform *= RotationPivotInverse.Value;
				if (PostRotation.HasValue)
					transform *= PostRotation.Value;
				if (rotation.HasValue)
					transform *= Matrix.CreateFromQuaternion(rotation.Value);
				else if (Rotation.HasValue)
					transform *= Rotation.Value;
				if (PreRotation.HasValue)
					transform *= PreRotation.Value;
				if (RotationPivot.HasValue)
					transform *= RotationPivot.Value;
				if (RotationOffset.HasValue)
					transform *= RotationOffset.Value;
				if (translation.HasValue)
					transform *= Matrix.CreateTranslation(translation.Value);
				else if (Translation.HasValue)
					transform *= Translation.Value;

				return transform;
			}
		}
		#endregion

		private static readonly List<VectorKey> EmptyVectorKeys = new List<VectorKey>();
		private static readonly List<QuaternionKey> EmptyQuaternionKeys = new List<QuaternionKey>();

		// Assimp scene
		private Scene _scene;
		private Dictionary<string, Matrix> _deformationBones;   // The names and offset matrices of all deformation bones.
		private Node _rootBone;                                 // The node that represents the root bone.
		private List<Node> _bones = new List<Node>();           // All nodes attached to the root bone.
		private Dictionary<string, FbxPivot> _pivots;              // The transformation pivots.

		// XNA content
		private ModelContent _result;

		private readonly string _importerName;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Importer()
			: this("Importer")
		{
		}

		internal Importer(string importerName)
		{
			_importerName = importerName;
		}

		/// <summary>
		/// This disables some Assimp model loading features so that
		/// the resulting content is the same as what the XNA FbxImporter 
		/// </summary>
		public bool XnaComptatible { get; set; }

		public ModelContent Import(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			if (CurrentPlatform.OS == OS.Linux)
			{
				var targetDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

				try
				{
					AssimpLibrary.Instance.LoadLibrary(
						Path.Combine(targetDir, "libassimp.so"),
						Path.Combine(targetDir, "libassimp.so"));
				}
				catch { }
			}

			_result = new ModelContent();
			using (var importer = new AssimpContext())
			{
				// FBXPreservePivotsConfig(false) can be set to remove transformation
				// pivots. However, Assimp does not automatically correct animations!
				// --> Leave default settings, handle transformation pivots explicitly.
				//importer.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));

				// Set flag to remove degenerate faces (points and lines).
				// This flag is very important when PostProcessSteps.FindDegenerates is used
				// because FindDegenerates converts degenerate triangles to points and lines!
				importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));

				// Note about Assimp post-processing:
				// Keep post-processing to a minimum. The ModelImporter should import
				// the model as is. We don't want to lose any information, i.e. empty
				// nodes shoud not be thrown away, meshes/materials should not be merged,
				// etc. Custom model processors may depend on this information!
				_scene = importer.ImportFile(filename,
					PostProcessSteps.FindDegenerates |
					PostProcessSteps.FindInvalidData |
					PostProcessSteps.FlipUVs |              // Required for Direct3D
					PostProcessSteps.FlipWindingOrder |     // Required for Direct3D
					PostProcessSteps.JoinIdenticalVertices |
					PostProcessSteps.ImproveCacheLocality |
					PostProcessSteps.OptimizeMeshes |
					PostProcessSteps.Triangulate

					// Unused: 
					//PostProcessSteps.CalculateTangentSpace
					//PostProcessSteps.Debone |
					//PostProcessSteps.FindInstances |      // No effect + slow?
					//PostProcessSteps.FixInFacingNormals |
					//PostProcessSteps.GenerateNormals |
					//PostProcessSteps.GenerateSmoothNormals |
					//PostProcessSteps.GenerateUVCoords | // Might be needed... find test case
					//PostProcessSteps.LimitBoneWeights |
					//PostProcessSteps.MakeLeftHanded |     // Not necessary, XNA is right-handed.
					//PostProcessSteps.OptimizeGraph |      // Will eliminate helper nodes
					//PostProcessSteps.PreTransformVertices |
					//PostProcessSteps.RemoveComponent |
					//PostProcessSteps.RemoveRedundantMaterials |
					//PostProcessSteps.SortByPrimitiveType |
					//PostProcessSteps.SplitByBoneCount |
					//PostProcessSteps.SplitLargeMeshes |
					//PostProcessSteps.TransformUVCoords |
					//PostProcessSteps.ValidateDataStructure |
					);

				// Create _materials.
				ImportMaterials();

				// Import meshes
				foreach(var mesh in _scene.Meshes)
				{
					var meshContent = ImportMesh(mesh);
					_result.Meshes.Add(meshContent);
				}

				//	ImportSkeleton();   // Create skeleton (incl. animations) and add to _rootNode.
			}
			return _result;
		}

		/// <summary>
		/// Converts all Assimp <see cref="Material"/>s to standard XNA compatible <see cref="MaterialContent"/>s.
		/// </summary>
		private void ImportMaterials()
		{
			foreach (var aiMaterial in _scene.Materials)
			{
				// TODO: What about AlphaTestMaterialContent, DualTextureMaterialContent, 
				// EffectMaterialContent, EnvironmentMapMaterialContent, and SkinnedMaterialContent?

				var material = new MaterialContent
				{
					Name = aiMaterial.Name,
				};

				if (aiMaterial.HasTextureDiffuse)
					material.Texture = ImportTextureContentRef(aiMaterial.TextureDiffuse);

				if (aiMaterial.HasTextureOpacity)
					material.TransparencyTexture = ImportTextureContentRef(aiMaterial.TextureOpacity);

				if (aiMaterial.HasTextureSpecular)
					material.SpecularTexture = ImportTextureContentRef(aiMaterial.TextureSpecular);

				if (aiMaterial.HasTextureHeight)
					material.BumpTexture = ImportTextureContentRef(aiMaterial.TextureHeight);

				if (aiMaterial.HasColorDiffuse)
					material.DiffuseColor = ToXna(aiMaterial.ColorDiffuse);

				if (aiMaterial.HasColorEmissive)
					material.EmissiveColor = ToXna(aiMaterial.ColorEmissive);

				if (aiMaterial.HasOpacity)
					material.Alpha = aiMaterial.Opacity;

				if (aiMaterial.HasColorSpecular)
					material.SpecularColor = ToXna(aiMaterial.ColorSpecular);

				if (aiMaterial.HasShininessStrength)
					material.SpecularPower = aiMaterial.Shininess;

				_result.Materials.Add(material);
			}
		}

		private TextureContent ImportTextureContentRef(TextureSlot textureSlot)
		{
			var texture = new TextureContent(textureSlot.FilePath);

			texture.UVIndex = textureSlot.UVIndex;
			texture.Operation = textureSlot.Operation;
			texture.WrapModeU = textureSlot.WrapModeU;
			texture.WrapModeV = textureSlot.WrapModeV;
			texture.Mapping = textureSlot.Mapping;

			return texture;
		}

		private MeshContent ImportMesh(Mesh aiMesh)
		{
			var result = new MeshContent
			{
				Material = _result.Materials[aiMesh.MaterialIndex]
			};

			// Vertices
			var elements = new List<VertexElement>();
			var offset = 0;
			var elementsPerRow = 0;
			elements.Add(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
			offset += 12;
			elementsPerRow += 3;

			// Colors
			for (var i = 0; i < aiMesh.VertexColorChannelCount; ++i)
			{
				elements.Add(new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0));
				offset += 3;
				elementsPerRow += 3;
			}

			if (aiMesh.HasNormals)
			{
				elements.Add(new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0));
				offset += 12;
				elementsPerRow += 3;
			}

			// Textures
			for (var i = 0; i < aiMesh.TextureCoordinateChannelCount; ++i)
			{
				elements.Add(new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
				offset += 8;
				elementsPerRow += 2;
			}

			result.ElementsPerRowWithoutBones = elementsPerRow;

			if (aiMesh.HasBones)
			{
				elements.Add(new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0));
				offset += 4;
				elementsPerRow += 4;

				elements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));
				offset += 16;
				elementsPerRow += 4;
			}
			result.ElementsPerRow = elementsPerRow;

			var data = new object[aiMesh.Vertices.Count, elementsPerRow];
			for (var i = 0; i < aiMesh.Vertices.Count; ++i)
			{
				var idx = 0;

				var v = ToXna(aiMesh.Vertices[i]);
				data[i, idx++] = v.X;
				data[i, idx++] = v.Y;
				data[i, idx++] = v.Z;

				// Colors
				for (var j = 0; j < aiMesh.VertexColorChannelCount; ++j)
				{
					v = Vector3.Zero;
					if (aiMesh.VertexColorChannels[j].Count > i)
					{
						v = ToXna(aiMesh.VertexColorChannels[0][i]);
					}
					data[i, idx++] = v.X;
					data[i, idx++] = v.Y;
					data[i, idx++] = v.Z;
				}

				// Normal
				if (aiMesh.HasNormals)
				{
					v = Vector3.Zero;
					if (aiMesh.Normals.Count > i)
					{
						v = ToXna(aiMesh.Normals[i]);
					}
					data[i, idx++] = v.X;
					data[i, idx++] = v.Y;
					data[i, idx++] = v.Z;
				}

				// Textures
				for (var j = 0; j < aiMesh.TextureCoordinateChannelCount; ++j)
				{
					v = Vector3.Zero;
					if (aiMesh.TextureCoordinateChannels[j].Count > i)
					{
						v = ToXna(aiMesh.TextureCoordinateChannels[0][i]);
					}
					data[i, idx++] = v.X;
					data[i, idx++] = v.Y;
				}

				if (aiMesh.HasBones)
				{
					// Filler values
					data[i, idx++] = 0;
					data[i, idx++] = 0;
					data[i, idx++] = 0;
					data[i, idx++] = 0;

					data[i, idx++] = 0.0f;
					data[i, idx++] = 0.0f;
					data[i, idx++] = 0.0f;
					data[i, idx++] = 0.0f;
				}
			}

			result.VertexDeclaration = new VertexDeclaration(elements.ToArray());
			result.Vertices = data;

			var indices = aiMesh.GetIndices();
			for (var i = 0; i < indices.Length; ++i)
			{
				var idx = indices[i];
				if (idx > short.MaxValue)
				{
					throw new Exception(string.Format("Only short indices supported. {0}", idx));
				}

				result.Indices.Add((short)idx);
			}

			if (aiMesh.HasBones)
			{
				var boneIds = new Dictionary<string, int>();
				foreach (var bone in aiMesh.Bones)
				{
					var boneContent = new BoneContent
					{
						BoneId = result.Bones.Count,
						Name = bone.Name,
						Transform = ToXna(bone.OffsetMatrix)
					};

					result.Bones.Add(boneContent);
					boneIds[bone.Name] = boneContent.BoneId;
				}

				var vertexIdxs = new Dictionary<int, int>();
				foreach (var bone in aiMesh.Bones)
				{
					var boneId = boneIds[bone.Name];
					foreach (var bw in bone.VertexWeights)
					{
						int idx;

						if (!vertexIdxs.TryGetValue(bw.VertexID, out idx))
						{
							idx = 0;
							vertexIdxs[bw.VertexID] = idx;
						}

						if (idx < 4)
						{
							result.Vertices[bw.VertexID, result.ElementsPerRowWithoutBones + idx] = boneId;
							result.Vertices[bw.VertexID, result.ElementsPerRowWithoutBones + idx + 4] = bw.Weight;

							++vertexIdxs[bw.VertexID];
						}
						else
						{
							Nrs.LogWarn("Vertex {0} has more than 4 bones.", bw.VertexID);
						}
					}
				}

				var bonesCount = 0;
				foreach (var pair in vertexIdxs)
				{
					if (pair.Value > bonesCount)
					{
						bonesCount = pair.Value;
					}
				}

				result.BonesCount = bonesCount;
			}
			return result;
		}

		/// <summary>
		/// Converts the specified animation to XNA.
		/// </summary>
		/// <param name="aiAnimation">The animation.</param>
		/// <param name="nodeName">An optional filter.</param>
		/// <returns>The animation converted to XNA.</returns>
		private AnimationContent ImportAnimation(Animation aiAnimation, string nodeName = null)
		{
			var animation = new AnimationContent
			{
				Name = GetAnimationName(aiAnimation.Name),
				Duration = TimeSpan.FromSeconds(aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond)
			};

			// In Assimp animation channels may be split into separate channels.
			//   "nodeXyz" --> "nodeXyz_$AssimpFbx$_Translation",
			//                 "nodeXyz_$AssimpFbx$_Rotation",
			//                 "nodeXyz_$AssimpFbx$_Scaling"
			// Group animation channels by name (strip the "_$AssimpFbx$" part).
			IEnumerable<IGrouping<string, NodeAnimationChannel>> channelGroups;
			if (nodeName != null)
			{
				channelGroups = aiAnimation.NodeAnimationChannels
										   .Where(channel => nodeName == GetNodeName(channel.NodeName))
										   .GroupBy(channel => GetNodeName(channel.NodeName));
			}
			else
			{
				channelGroups = aiAnimation.NodeAnimationChannels
										   .GroupBy(channel => GetNodeName(channel.NodeName));
			}

			foreach (var channelGroup in channelGroups)
			{
				var boneName = channelGroup.Key;
				var channel = new AnimationChannel();

				// Get transformation pivot for current bone.
				FbxPivot pivot;
				if (!_pivots.TryGetValue(boneName, out pivot))
					pivot = FbxPivot.Default;

				var scaleKeys = EmptyVectorKeys;
				var rotationKeys = EmptyQuaternionKeys;
				var translationKeys = EmptyVectorKeys;

				foreach (var aiChannel in channelGroup)
				{
					if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Scaling"))
					{
						scaleKeys = aiChannel.ScalingKeys;

						Debug.Assert(pivot.Scaling.HasValue);
						Debug.Assert(!aiChannel.HasRotationKeys || (aiChannel.RotationKeyCount == 1 && (aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(1, 0, 0, 0) || aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(0, 0, 0, 0))));
						Debug.Assert(!aiChannel.HasPositionKeys || (aiChannel.PositionKeyCount == 1 && aiChannel.PositionKeys[0].Value == new Vector3D(0, 0, 0)));
					}
					else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Rotation"))
					{
						rotationKeys = aiChannel.RotationKeys;

						Debug.Assert(pivot.Rotation.HasValue);
						Debug.Assert(!aiChannel.HasScalingKeys || (aiChannel.ScalingKeyCount == 1 && aiChannel.ScalingKeys[0].Value == new Vector3D(1, 1, 1)));
						Debug.Assert(!aiChannel.HasPositionKeys || (aiChannel.PositionKeyCount == 1 && aiChannel.PositionKeys[0].Value == new Vector3D(0, 0, 0)));
					}
					else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Translation"))
					{
						translationKeys = aiChannel.PositionKeys;

						Debug.Assert(pivot.Translation.HasValue);
						Debug.Assert(!aiChannel.HasScalingKeys || (aiChannel.ScalingKeyCount == 1 && aiChannel.ScalingKeys[0].Value == new Vector3D(1, 1, 1)));
						Debug.Assert(!aiChannel.HasRotationKeys || (aiChannel.RotationKeyCount == 1 && (aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(1, 0, 0, 0) || aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(0, 0, 0, 0))));
					}
					else
					{
						scaleKeys = aiChannel.ScalingKeys;
						rotationKeys = aiChannel.RotationKeys;
						translationKeys = aiChannel.PositionKeys;
					}
				}

				// Get all unique keyframe times. (Assuming that no two key frames
				// have the same time, which is usually a safe assumption.)
				var times = scaleKeys.Select(k => k.Time)
									 .Union(rotationKeys.Select(k => k.Time))
									 .Union(translationKeys.Select(k => k.Time))
									 .OrderBy(t => t)
									 .ToList();

				Debug.Assert(times.Count == times.Distinct().Count(), "Sequences combined with Union() should not have duplicates.");

				int prevScaleIndex = -1;
				int prevRotationIndex = -1;
				int prevTranslationIndex = -1;
				double prevScaleTime = 0.0;
				double prevRotationTime = 0.0;
				double prevTranslationTime = 0.0;
				Vector3? prevScale = null;
				Quaternion? prevRotation = null;
				Vector3? prevTranslation = null;

				foreach (var time in times)
				{
					// Get scaling.
					Vector3? scale;
					int scaleIndex = scaleKeys.FindIndex(k => k.Time == time);
					if (scaleIndex != -1)
					{
						// Scaling key found.
						scale = ToXna(scaleKeys[scaleIndex].Value);
						prevScaleIndex = scaleIndex;
						prevScaleTime = time;
						prevScale = scale;
					}
					else
					{
						// No scaling key found.
						if (prevScaleIndex != -1 && prevScaleIndex + 1 < scaleKeys.Count)
						{
							// Lerp between previous and next scaling key.
							var nextScaleKey = scaleKeys[prevScaleIndex + 1];
							var nextScaleTime = nextScaleKey.Time;
							var nextScale = ToXna(nextScaleKey.Value);
							var amount = (float)((time - prevScaleTime) / (nextScaleTime - prevScaleTime));
							scale = Vector3.Lerp(prevScale.Value, nextScale, amount);
						}
						else
						{
							// Hold previous scaling value.
							scale = prevScale;
						}
					}

					// Get rotation.
					Quaternion? rotation;
					int rotationIndex = rotationKeys.FindIndex(k => k.Time == time);
					if (rotationIndex != -1)
					{
						// Rotation key found.
						rotation = ToXna(rotationKeys[rotationIndex].Value);
						prevRotationIndex = rotationIndex;
						prevRotationTime = time;
						prevRotation = rotation;
					}
					else
					{
						// No rotation key found.
						if (prevRotationIndex != -1 && prevRotationIndex + 1 < rotationKeys.Count)
						{
							// Lerp between previous and next rotation key.
							var nextRotationKey = rotationKeys[prevRotationIndex + 1];
							var nextRotationTime = nextRotationKey.Time;
							var nextRotation = ToXna(nextRotationKey.Value);
							var amount = (float)((time - prevRotationTime) / (nextRotationTime - prevRotationTime));
							rotation = Quaternion.Slerp(prevRotation.Value, nextRotation, amount);
						}
						else
						{
							// Hold previous rotation value.
							rotation = prevRotation;
						}
					}

					// Get translation.
					Vector3? translation;
					int translationIndex = translationKeys.FindIndex(k => k.Time == time);
					if (translationIndex != -1)
					{
						// Translation key found.
						translation = ToXna(translationKeys[translationIndex].Value);
						prevTranslationIndex = translationIndex;
						prevTranslationTime = time;
						prevTranslation = translation;
					}
					else
					{
						// No translation key found.
						if (prevTranslationIndex != -1 && prevTranslationIndex + 1 < translationKeys.Count)
						{
							// Lerp between previous and next translation key.
							var nextTranslationKey = translationKeys[prevTranslationIndex + 1];
							var nextTranslationTime = nextTranslationKey.Time;
							var nextTranslation = ToXna(nextTranslationKey.Value);
							var amount = (float)((time - prevTranslationTime) / (nextTranslationTime - prevTranslationTime));
							translation = Vector3.Lerp(prevTranslation.Value, nextTranslation, amount);
						}
						else
						{
							// Hold previous translation value.
							translation = prevTranslation;
						}
					}

					// Apply transformation pivot.
					var transform = pivot.GetTransform(scale, rotation, translation);

					long ticks = (long)(time * (TimeSpan.TicksPerSecond / aiAnimation.TicksPerSecond));
					channel.Add(new AnimationKeyframe(TimeSpan.FromTicks(ticks), transform));
				}

				animation.Channels[channelGroup.Key] = channel;
			}

			return animation;
		}

		/// <summary>
		/// Copies the current node and all descendant nodes into a list.
		/// </summary>
		/// <param name="node">The current node.</param>
		/// <param name="list">The list.</param>
		private static void GetSubtree(Node node, List<Node> list)
		{
			Debug.Assert(node != null);
			Debug.Assert(list != null);

			list.Add(node);
			foreach (var child in node.Children)
				GetSubtree(child, list);
		}

		/// <summary>
		/// Gets the transform of node relative to a specific ancestor node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="ancestor">The ancestor node. Can be <see langword="null"/>.</param>
		/// <returns>
		/// The relative transform. If <paramref name="ancestor"/> is <see langword="null"/> the
		/// absolute transform of <paramref name="node"/> is returned.
		/// </returns>
		private static Matrix4x4 GetRelativeTransform(Node node, Node ancestor)
		{
			Debug.Assert(node != null);

			// Get transform of node relative to ancestor.
			Matrix4x4 transform = node.Transform;
			Node parent = node.Parent;
			while (parent != null && parent != ancestor)
			{
				transform *= parent.Transform;
				parent = parent.Parent;
			}

			if (parent == null && ancestor != null)
				throw new ArgumentException(string.Format("Node \"{0}\" is not an ancestor of \"{1}\".", ancestor.Name, node.Name));

			return transform;
		}

		/// <summary>
		/// Gets the animation name without the "AnimStack::" part.
		/// </summary>
		/// <param name="name">The mangled animation name.</param>
		/// <returns>The original animation name.</returns>
		private static string GetAnimationName(string name)
		{
			return name.Replace("AnimStack::", string.Empty);
		}

		/// <summary>
		/// Gets the node name without the "_$AssimpFbx$" part.
		/// </summary>
		/// <param name="name">The mangled node name.</param>
		/// <returns>The original node name.</returns>
		private static string GetNodeName(string name)
		{
			int index = name.IndexOf("_$AssimpFbx$", StringComparison.Ordinal);
			return (index >= 0) ? name.Remove(index) : name;
		}

		#region Conversion Helpers

		[DebuggerStepThrough]
		public static Matrix ToXna(Matrix4x4 matrix)
		{
			var result = Matrix.Identity;

			result.M11 = matrix.A1;
			result.M12 = matrix.B1;
			result.M13 = matrix.C1;
			result.M14 = matrix.D1;

			result.M21 = matrix.A2;
			result.M22 = matrix.B2;
			result.M23 = matrix.C2;
			result.M24 = matrix.D2;

			result.M31 = matrix.A3;
			result.M32 = matrix.B3;
			result.M33 = matrix.C3;
			result.M34 = matrix.D3;

			result.M41 = matrix.A4;
			result.M42 = matrix.B4;
			result.M43 = matrix.C4;
			result.M44 = matrix.D4;

			return result;
		}

		[DebuggerStepThrough]
		public static Vector2 ToXna(Vector2D vector)
		{
			return new Vector2(vector.X, vector.Y);
		}

		[DebuggerStepThrough]
		public static Vector3 ToXna(Vector3D vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}

		[DebuggerStepThrough]
		public static Quaternion ToXna(Assimp.Quaternion quaternion)
		{
			return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
		}

		[DebuggerStepThrough]
		public static Vector3 ToXna(Color4D color)
		{
			return new Vector3(color.R, color.G, color.B);
		}

		[DebuggerStepThrough]
		public static Vector2 ToXnaTexCoord(Vector3D vector)
		{
			return new Vector2(vector.X, vector.Y);
		}

		[DebuggerStepThrough]
		public static Color ToXnaColor(Color4D color)
		{
			return new Color(color.R, color.G, color.B, color.A);
		}
		#endregion
	}
}