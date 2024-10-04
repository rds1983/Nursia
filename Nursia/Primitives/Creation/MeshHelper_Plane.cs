// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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

using Nursia.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Data.Meshes
{
	/// <summary>
	/// Enumerates the different possible direction of a plane normal.
	/// </summary>
	public enum NormalDirection
	{
		UpZ,
		UpY,
		UpX,
	}

	partial class MeshHelper
	{
		/// <summary>
		/// Creates a Plane primitive on the X/Y plane with a normal equal to -<see cref="Vector3.UnitZ"/>.
		/// </summary>
		/// <param name="sizeX">The size X.</param>
		/// <param name="sizeY">The size Y.</param>
		/// <param name="tessellationX">The tessellation, as the number of quads per X axis.</param>
		/// <param name="tessellationY">The tessellation, as the number of quads per Y axis.</param>
		/// <param name="uScale">Scale U coordinates between 0 and the values of this parameter.</param>
		/// <param name="vScale">Scale V coordinates 0 and the values of this parameter.</param>
		/// <param name="generateBackFace">Add a back face to the plane</param>
		/// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
		/// <param name="normalDirection">The direction of the plane normal</param>
		/// <returns>A Plane primitive.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">tessellationX;tessellation must be > 0</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">tessellationY;tessellation must be > 0</exception>
		public static Mesh CreatePlane(float sizeX = 1.0f, float sizeY = 1.0f, int tessellationX = 1, int tessellationY = 1, float uScale = 1f, float vScale = 1f, bool generateBackFace = false, bool toLeftHanded = false, NormalDirection normalDirection = 0)
		{
			if (tessellationX < 1)
			{
				throw new ArgumentOutOfRangeException("tessellationX", "tessellation must be > 0");
			}

			if (tessellationY < 1)
			{
				throw new ArgumentOutOfRangeException("tessellationY", "tessellation must be > 0");
			}

			var lineWidth = tessellationX + 1;
			var lineHeight = tessellationY + 1;

			var deltaX = sizeX / tessellationX;
			var deltaY = sizeY / tessellationY;

			sizeX /= 2.0f;
			sizeY /= 2.0f;

			var builder = new MeshBuilder();

			Vector3 normal;
			switch (normalDirection)
			{
				default:
				case NormalDirection.UpZ: normal = Vector3.UnitZ; break;
				case NormalDirection.UpY: normal = Vector3.UnitY; break;
				case NormalDirection.UpX: normal = Vector3.UnitX; break;
			}

			var uv = new Vector2(uScale, vScale);

			// Create vertices
			for (int y = 0; y < (tessellationY + 1); y++)
			{
				for (int x = 0; x < (tessellationX + 1); x++)
				{
					Vector3 position;
					switch (normalDirection)
					{
						default:
						case NormalDirection.UpZ: position = new Vector3(-sizeX + deltaX * x, sizeY - deltaY * y, 0); break;
						case NormalDirection.UpY: position = new Vector3(-sizeX + deltaX * x, 0, -sizeY + deltaY * y); break;
						case NormalDirection.UpX: position = new Vector3(0, sizeY - deltaY * y, sizeX - deltaX * x); break;
					}
					var texCoord = new Vector2(uv.X * x / tessellationX, uv.Y * y / tessellationY);
					builder.Vertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
				}
			}

			// Create indices
			for (int y = 0; y < tessellationY; y++)
			{
				for (int x = 0; x < tessellationX; x++)
				{
					// Six indices (two triangles) per face.
					int vbase = lineWidth * y + x;
					builder.AddIndex(vbase + 1);
					builder.AddIndex(vbase + 1 + lineWidth);
					builder.AddIndex(vbase + lineWidth);

					builder.AddIndex(vbase + 1);
					builder.AddIndex(vbase + lineWidth);
					builder.AddIndex(vbase);
				}
			}

			if (generateBackFace)
			{
				var numVertices = lineWidth * lineHeight;
				normal = -normal;
				for (int y = 0; y < (tessellationY + 1); y++)
				{
					for (int x = 0; x < (tessellationX + 1); x++)
					{
						var baseVertex = builder.Vertices[builder.Vertices.Count - numVertices];
						var position = new Vector3(baseVertex.Position.X, baseVertex.Position.Y, baseVertex.Position.Z);
						var texCoord = new Vector2(uv.X * x / tessellationX, uv.Y * y / tessellationY);
						builder.Vertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
					}
				}
				// Create indices
				for (int y = 0; y < tessellationY; y++)
				{
					for (int x = 0; x < tessellationX; x++)
					{
						// Six indices (two triangles) per face.
						int vbase = lineWidth * (y + tessellationY + 1) + x;
						builder.AddIndex(vbase + 1);
						builder.AddIndex(vbase + lineWidth);
						builder.AddIndex(vbase + 1 + lineWidth);

						builder.AddIndex(vbase + 1);
						builder.AddIndex(vbase);
						builder.AddIndex(vbase + lineWidth);
					}
				}
			}

			// Create the primitive object.
			return builder.Create(toLeftHanded);
		}
	}
}
