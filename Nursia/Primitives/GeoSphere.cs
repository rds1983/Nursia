// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTk http://directxtk.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using Nursia.Rendering;

namespace Nursia.Primitives
{
	public class GeoSphere : PrimitiveMesh
	{
		private static readonly Vector3[] OctahedronVertices =
		{
			// when looking down the negative z-axis (into the screen)
			new Vector3(+0,  1,  0), // 0 top
			new Vector3(+0,  0, -1), // 1 front
			new Vector3(+1,  0,  0), // 2 right
			new Vector3(+0,  0,  1), // 3 back
			new Vector3(-1,  0,  0), // 4 left
			new Vector3(+0, -1,  0), // 5 bottom
		};

		private static readonly short[] OctahedronIndices =
		{
			0, 1, 2, // top front-right face
			0, 2, 3, // top back-right face
			0, 3, 4, // top back-left face
			0, 4, 1, // top front-left face
			5, 1, 4, // bottom front-left face
			5, 4, 3, // bottom back-left face
			5, 3, 2, // bottom back-right face
			5, 2, 1, // bottom front-right face
		};

		private unsafe class GeoSphereBuilder : Builder
		{
			public List<Vector3> Positions { get; } = new List<Vector3>(OctahedronVertices);

			public short* IndicesPtr;

			// Key: an edge
			// Value: the index of the vertex which lies midway between the two vertices pointed to by the key value
			// This map is used to avoid duplicating vertices when subdividing triangles along edges.
			public Dictionary<UndirectedEdge, short> SubdividedEdges { get; } = new Dictionary<UndirectedEdge, short>();

			public GeoSphereBuilder()
			{
				Indices.AddRange(OctahedronIndices);
			}
		}

		private float _radius = 0.5f;
		private int _tessellation = 3;

		public float Radius
		{
			get => _radius;

			set
			{
				if (value.EpsilonEquals(_radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		protected unsafe override Mesh CreateMesh()
		{
			var builder = new GeoSphereBuilder();

			// We know these values by looking at the above index list for the octahedron. Despite the subdivisions that are
			// about to go on, these values aren't ever going to change because the vertices don't move around in the array.
			// We'll need these values later on to fix the singularities that show up at the poles.
			const int northPoleIndex = 0;
			const int southPoleIndex = 5;

			for (int iSubdivision = 0; iSubdivision < _tessellation; ++iSubdivision)
			{
				// The new index collection after subdivision.
				var newIndices = new List<short>();
				builder.SubdividedEdges.Clear();

				int triangleCount = builder.Indices.Count / 3;
				for (int iTriangle = 0; iTriangle < triangleCount; ++iTriangle)
				{
					// For each edge on this triangle, create a new vertex in the middle of that edge.
					// The winding order of the triangles we output are the same as the winding order of the inputs.

					// Indices of the vertices making up this triangle
					var iv0 = builder.Indices[iTriangle * 3 + 0];
					var iv1 = builder.Indices[iTriangle * 3 + 1];
					var iv2 = builder.Indices[iTriangle * 3 + 2];

					//// The existing vertices
					//Vector3 v0 = builder.Positions[iv0];
					//Vector3 v1 = builder.Positions[iv1];
					//Vector3 v2 = builder.Positions[iv2];

					// Get the new vertices
					Vector3 v01; // vertex on the midpoint of v0 and v1
					Vector3 v12; // ditto v1 and v2
					Vector3 v20; // ditto v2 and v0
					short iv01; // index of v01
					short iv12; // index of v12
					short iv20; // index of v20

					// Add/get new vertices and their indices
					DivideEdge(builder, iv0, iv1, out v01, out iv01);
					DivideEdge(builder, iv1, iv2, out v12, out iv12);
					DivideEdge(builder, iv0, iv2, out v20, out iv20);

					// Add the new indices. We have four new triangles from our original one:
					//        v0
					//        o
					//       /a\
					//  v20 o---o v01
					//     /b\c/d\
					// v2 o---o---o v1
					//       v12

					// a
					newIndices.Add(iv0);
					newIndices.Add(iv01);
					newIndices.Add(iv20);

					// b
					newIndices.Add(iv20);
					newIndices.Add(iv12);
					newIndices.Add(iv2);

					// d
					newIndices.Add(iv20);
					newIndices.Add(iv01);
					newIndices.Add(iv12);

					// d
					newIndices.Add(iv01);
					newIndices.Add(iv1);
					newIndices.Add(iv12);
				}

				builder.Indices.Clear();
				builder.Indices.AddRange(newIndices);
			}

			// Now that we've completed subdivision, fill in the final vertex collection
			builder.Vertices = new List<VertexPositionNormalTexture>(builder.Positions.Count);
			for (int i = 0; i < builder.Positions.Count; i++)
			{
				var vertexValue = builder.Positions[i];

				var normal = vertexValue;
				normal.Normalize();

				var pos = normal * _radius;

				// calculate texture coordinates for this vertex
				float longitude = MathF.Atan2(normal.X, -normal.Z);
				float latitude = MathF.Acos(normal.Y);

				float u = (float)(longitude / (Math.PI * 2.0) + 0.5);
				float v = (float)(latitude / Math.PI);

				var texcoord = new Vector2(1.0f - u, v);
				texcoord *= new Vector2(UScale, VScale);
				builder.Vertices.Add(new VertexPositionNormalTexture(pos, normal, texcoord));
			}

			const float XMVectorSplatEpsilon = 1.192092896e-7f;

			// There are a couple of fixes to do. One is a texture coordinate wraparound fixup. At some point, there will be
			// a set of triangles somewhere in the mesh with texture coordinates such that the wraparound across 0.0/1.0
			// occurs across that triangle. Eg. when the left hand side of the triangle has a U coordinate of 0.98 and the
			// right hand side has a U coordinate of 0.0. The intent is that such a triangle should render with a U of 0.98 to
			// 1.0, not 0.98 to 0.0. If we don't do this fixup, there will be a visible seam across one side of the sphere.
			// 
			// Luckily this is relatively easy to fix. There is a straight edge which runs down the prime meridian of the
			// completed sphere. If you imagine the vertices along that edge, they circumscribe a semicircular arc starting at
			// y=1 and ending at y=-1, and sweeping across the range of z=0 to z=1. x stays zero. It's along this edge that we
			// need to duplicate our vertices - and provide the correct texture coordinates.
			int preCount = builder.Vertices.Count;
			var indicesArray = builder.Indices.ToArray();
			fixed (void* pIndices = indicesArray)
			{
				builder.IndicesPtr = (short*)pIndices;

				for (int i = 0; i < preCount; ++i)
				{
					// Poles will be fixed separately
					if (i == southPoleIndex || i == northPoleIndex)
						continue;

					// This vertex is on the prime meridian if position.x and texcoord.u are both zero (allowing for small epsilon).
					bool isOnPrimeMeridian = Mathematics.EpsilonEquals(builder.Vertices[i].Position.X, 0, XMVectorSplatEpsilon)
											 && Mathematics.EpsilonEquals(builder.Vertices[i].TextureCoordinate.X, 0, XMVectorSplatEpsilon);

					if (isOnPrimeMeridian)
					{
						int newIndex = builder.Vertices.Count;

						// copy this vertex, correct the texture coordinate, and add the vertex
						VertexPositionNormalTexture v = builder.Vertices[i];
						v.TextureCoordinate.X = UScale;
						builder.Vertices.Add(v);

						// Now find all the triangles which contain this vertex and update them if necessary
						for (int j = 0; j < builder.Indices.Count; j += 3)
						{
							var triIndex0 = &builder.IndicesPtr[j + 0];
							var triIndex1 = &builder.IndicesPtr[j + 1];
							var triIndex2 = &builder.IndicesPtr[j + 2];

							if (*triIndex0 == i)
							{
								// nothing; just keep going
							}
							else if (*triIndex1 == i)
							{
								Utils.Swap(ref *triIndex0, ref *triIndex1);
								Utils.Swap(ref *triIndex1, ref *triIndex2);
							}
							else if (*triIndex2 == i)
							{
								Utils.Swap(ref *triIndex0, ref *triIndex2);
								Utils.Swap(ref *triIndex2, ref *triIndex1);
							}
							else
							{
								// this triangle doesn't use the vertex we're interested in
								continue;
							}

							// check the other two vertices to see if we might need to fix this triangle
							if (Math.Abs(builder.Vertices[*triIndex0].TextureCoordinate.X - builder.Vertices[*triIndex1].TextureCoordinate.X) > 0.5f * UScale ||
								Math.Abs(builder.Vertices[*triIndex0].TextureCoordinate.X - builder.Vertices[*triIndex2].TextureCoordinate.X) > 0.5f * UScale)
							{
								// yep; replace the specified index to point to the new, corrected vertex
								builder.IndicesPtr[j + 0] = (short)newIndex;
							}
						}
					}
				}

				FixPole(builder, northPoleIndex);
				FixPole(builder, southPoleIndex);

				// Clear indices as it will not be accessible outside the fixed statement
				builder.IndicesPtr = (short*)0;
			}

			return builder.Create(IsLeftHanded);
		}

		private unsafe void FixPole(GeoSphereBuilder builder, int poleIndex)
		{
			var poleVertex = builder.Vertices[poleIndex];
			bool overwrittenPoleVertex = false; // overwriting the original pole vertex saves us one vertex

			for (ushort i = 0; i < builder.Indices.Count; i += 3)
			{
				// These pointers point to the three indices which make up this triangle. pPoleIndex is the pointer to the
				// entry in the index array which represents the pole index, and the other two pointers point to the other
				// two indices making up this triangle.
				short* pPoleIndex;
				short* pOtherIndex0;
				short* pOtherIndex1;
				if (builder.IndicesPtr[i + 0] == poleIndex)
				{
					pPoleIndex = &builder.IndicesPtr[i + 0];
					pOtherIndex0 = &builder.IndicesPtr[i + 1];
					pOtherIndex1 = &builder.IndicesPtr[i + 2];
				}
				else if (builder.IndicesPtr[i + 1] == poleIndex)
				{
					pPoleIndex = &builder.IndicesPtr[i + 1];
					pOtherIndex0 = &builder.IndicesPtr[i + 2];
					pOtherIndex1 = &builder.IndicesPtr[i + 0];
				}
				else if (builder.IndicesPtr[i + 2] == poleIndex)
				{
					pPoleIndex = &builder.IndicesPtr[i + 2];
					pOtherIndex0 = &builder.IndicesPtr[i + 0];
					pOtherIndex1 = &builder.IndicesPtr[i + 1];
				}
				else
				{
					continue;
				}

				// Calculate the texcoords for the new pole vertex, add it to the vertices and update the index
				var newPoleVertex = poleVertex;
				newPoleVertex.TextureCoordinate.X = (builder.Vertices[*pOtherIndex0].TextureCoordinate.X + builder.Vertices[*pOtherIndex1].TextureCoordinate.X) * 0.5f;
				newPoleVertex.TextureCoordinate.Y = poleVertex.TextureCoordinate.Y;

				if (!overwrittenPoleVertex)
				{
					builder.Vertices[poleIndex] = newPoleVertex;
					overwrittenPoleVertex = true;
				}
				else
				{
					*pPoleIndex = (short)builder.Vertices.Count;
					builder.Vertices.Add(newPoleVertex);
				}
			}
		}

		// Function that, when given the index of two vertices, creates a new vertex at the midpoint of those builder.Vertices.
		private void DivideEdge(GeoSphereBuilder builder, int i0, int i1, out Vector3 outVertex, out short outIndex)
		{
			var edge = new UndirectedEdge(i0, i1);

			// Check to see if we've already generated this vertex
			if (builder.SubdividedEdges.TryGetValue(edge, out outIndex))
			{
				// We've already generated this vertex before
				outVertex = builder.Positions[outIndex]; // and the vertex itself
			}
			else
			{
				// Haven't generated this vertex before: so add it now

				// outVertex = (builder.Vertices[i0] + builder.Vertices[i1]) / 2
				outVertex = (builder.Positions[i0] + builder.Positions[i1]) * 0.5f;
				outIndex = (short)builder.Positions.Count;
				builder.Positions.Add(outVertex);

				// Now add it to the map.
				builder.SubdividedEdges[edge] = outIndex;
			}
		}

		// An undirected edge between two vertices, represented by a pair of indexes into a vertex array.
		// Because this edge is undirected, (a,b) is the same as (b,a).
		private struct UndirectedEdge : IEquatable<UndirectedEdge>
		{
			public UndirectedEdge(int item1, int item2)
			{
				// Makes an undirected edge. Rather than overloading comparison operators to give us the (a,b)==(b,a) property,
				// we'll just ensure that the larger of the two goes first. This'll simplify things greatly.
				Item1 = Math.Max(item1, item2);
				Item2 = Math.Min(item1, item2);
			}

			public readonly int Item1;

			public readonly int Item2;

			public bool Equals(UndirectedEdge other)
			{
				return Item1 == other.Item1 && Item2 == other.Item2;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				return obj is UndirectedEdge && Equals((UndirectedEdge)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (Item1.GetHashCode() * 397) ^ Item2.GetHashCode();
				}
			}

			public static bool operator ==(UndirectedEdge left, UndirectedEdge right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(UndirectedEdge left, UndirectedEdge right)
			{
				return !left.Equals(right);
			}
		}
	}
}