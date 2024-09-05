// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;
using System;

namespace Nursia.Primitives
{
	/// <summary>
	/// A disc - a circular base, or a circular sector.
	/// </summary>
	public class Disc : PrimitiveMesh
	{
		private float _radius = 0.5f;
		private float _sectorAngle = 2 * MathF.PI;
		private int _tessellation = 16;

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

		public float SectorAngle
		{
			get => _sectorAngle;

			set
			{
				if (value.EpsilonEquals(_sectorAngle))
				{
					return;
				}

				_sectorAngle = value;
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

		protected override Mesh CreateMesh()
		{
			if (_tessellation < 4)
				throw new ArgumentOutOfRangeException("tessellation", "tessellation parameter out of range");

			var numberOfSections = _tessellation + 2;

			var builder = new Builder();

			// f in {-1, 1} - two faces
			for (var f = -1; f < 2; f += 2)
			{
				var normal = new Vector3(0, f, 0);

				// center point
				builder.Vertices.Add(new VertexPositionNormalTexture { Position = new Vector3(), Normal = normal, TextureCoordinate = new Vector2() });

				// edge points
				for (var i = 0; i <= _tessellation; ++i)
				{
					var angle = i / (double)_tessellation * _sectorAngle;
					// FIXME: I don't really know how to set up texture coordinates in a sane way
					var textureCoordinate = new Vector2((float)i / _tessellation, 1);
					textureCoordinate.X *= UScale;
					textureCoordinate.Y *= VScale;
					var position = new Vector3((float)Math.Cos(angle) * _radius, 0, (float)Math.Sin(angle) * _radius);

					builder.Vertices.Add(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
				}
			}

			var secondFaceOffset = numberOfSections;

			// the indices
			for (int i = 1; i <= _tessellation; ++i)
			{
				builder.Indices.Add((short)(0));
				builder.Indices.Add((short)(i));
				builder.Indices.Add((short)(i + 1));

				// note the opposite order of vertices - this is required to make the second face look the other way
				builder.Indices.Add((short)(secondFaceOffset + i + 1));
				builder.Indices.Add((short)(secondFaceOffset + i));
				builder.Indices.Add((short)(secondFaceOffset + 0));
			}

			return builder.Create(IsLeftHanded);
		}
	}
}
