// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Nursia.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Data.Meshes
{
	partial class MeshHelper
	{
		/// <summary>
		/// Creates a disc.
		/// </summary>
		/// <param name="radius">The radius of the base</param>
		/// <param name="sectorAngle">The angle of the circular sector</param>
		/// <param name="tessellation">The number of segments composing the base</param>
		/// <param name="uScale">Scale U coordinates between 0 and the values of this parameter.</param>
		/// <param name="vScale">Scale V coordinates 0 and the values of this parameter.</param>
		/// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
		/// <returns>A cone.</returns>
		public static Mesh CreateDisc(float radius = 0.5f, float sectorAngle = 2 * MathF.PI, int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
		{
			if (tessellation < 4)
				tessellation = 4;

			var numberOfSections = tessellation + 2;

			var builder = new MeshBuilder();

			// f in {-1, 1} - two faces
			for (var f = -1; f < 2; f += 2)
			{
				var normal = new Vector3(0, f, 0);

				// center point
				builder.Vertices.Add(new VertexPositionNormalTexture { Position = new Vector3(), Normal = normal, TextureCoordinate = new Vector2() });

				// edge points
				for (var i = 0; i <= tessellation; ++i)
				{
					var angle = i / (double)tessellation * sectorAngle;
					// FIXME: I don't really know how to set up texture coordinates in a sane way
					var textureCoordinate = new Vector2((float)i / tessellation, 1);
					textureCoordinate.X *= uScale;
					textureCoordinate.Y *= vScale;
					var position = new Vector3((float)Math.Cos(angle) * radius, 0, (float)Math.Sin(angle) * radius);

					builder.Vertices.Add(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
				}
			}

			var secondFaceOffset = numberOfSections;

			// the indices
			for (int i = 1; i <= tessellation; ++i)
			{
				builder.AddIndex(0);
				builder.AddIndex(i);
				builder.AddIndex(i + 1);

				// note the opposite order of vertices - this is required to make the second face look the other way
				builder.AddIndex(secondFaceOffset + i + 1);
				builder.AddIndex(secondFaceOffset + i);
				builder.AddIndex(secondFaceOffset + 0);
			}

			return builder.Create(toLeftHanded);
		}
	}
}
