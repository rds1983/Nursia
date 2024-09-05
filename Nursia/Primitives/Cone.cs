using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;
using System;

namespace Nursia.Primitives
{
	public class Cone : PrimitiveMesh
	{
		private float _radius = 0.5f;
		private float _height = 1.0f;
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

		public float Height
		{
			get => _height;

			set
			{
				if (value.EpsilonEquals(_height))
				{
					return;
				}

				_height = value;
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
			if (_tessellation < 3)
				throw new ArgumentOutOfRangeException("tessellation", "tessellation parameter out of range");

			var builder = new Builder();
			var numberOfSections = _tessellation + 1;
			var vertexNumberBySection = _tessellation + 1;

			// e == 0 => Cone 
			// e == 1 => Cap
			for (var e = 0; e < 2; e++)
			{
				var topHeight = e == 0 ? _height : 0;
				var normalSign = Math.Sign(0.5 - e);
				var slopeLength = Math.Sqrt(_radius * _radius + topHeight * topHeight);
				var slopeCos = topHeight / slopeLength;
				var slopeSin = _radius / slopeLength;

				// the cone sections
				for (var j = 0; j < _tessellation; ++j)
				{
					var sectionRatio = j / (float)_tessellation;
					var sectionHeight = (sectionRatio * topHeight) - _height * 0.5f;
					var sectionRadius = (1 - sectionRatio) * _radius;

					for (var i = 0; i <= _tessellation; ++i)
					{
						var angle = i / (double)_tessellation * 2.0 * Math.PI;
						var textureCoordinate = new Vector2((float)i / _tessellation, 1 - sectionRatio);
						textureCoordinate.X *= UScale;
						textureCoordinate.Y *= VScale;
						var position = new Vector3((float)Math.Cos(angle) * sectionRadius, sectionHeight, (float)Math.Sin(angle) * sectionRadius);
						var normal = normalSign * new Vector3((float)(Math.Cos(angle) * slopeCos), (float)slopeSin, (float)(Math.Sin(angle) * slopeCos));

						builder.Vertices.Add(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
					}
				}

				// the extremity points
				for (var i = 0; i <= _tessellation; ++i)
				{
					var position = new Vector3(0, topHeight - _height * 0.5f, 0);
					var angle = (i + 0.5) / _tessellation * 2.0 * Math.PI;
					var textureCoordinate = new Vector2((i + 0.5f) / _tessellation, 0);
					textureCoordinate.X *= UScale;
					textureCoordinate.Y *= VScale;
					var normal = normalSign * new Vector3((float)(Math.Cos(angle) * slopeCos), (float)slopeSin, (float)(Math.Sin(angle) * slopeCos));

					builder.Vertices.Add(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
				}
			}

			// the indices
			for (var e = 0; e < 2; e++)
			{
				var globalOffset = (e == 0) ? 0 : vertexNumberBySection * numberOfSections;
				var offsetV1 = (e == 0) ? 1 : vertexNumberBySection;
				var offsetV2 = (e == 0) ? vertexNumberBySection : 1;
				var offsetV3 = (e == 0) ? 1 : vertexNumberBySection + 1;
				var offsetV4 = (e == 0) ? vertexNumberBySection + 1 : 1;

				// the sections
				for (var j = 0; j < _tessellation - 1; ++j)
				{
					for (int i = 0; i < _tessellation; ++i)
					{
						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i));
						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i + offsetV1));
						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i + offsetV2));

						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i + vertexNumberBySection));
						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i + offsetV3));
						builder.Indices.Add((short)(globalOffset + j * vertexNumberBySection + i + offsetV4));
					}
				}

				// the extremity triangle
				for (int i = 0; i < _tessellation; ++i)
				{
					builder.Indices.Add((short)(globalOffset + (_tessellation - 1) * vertexNumberBySection + i));
					builder.Indices.Add((short)(globalOffset + (_tessellation - 1) * vertexNumberBySection + i + offsetV1));
					builder.Indices.Add((short)(globalOffset + (_tessellation - 1) * vertexNumberBySection + i + offsetV2));
				}
			}

			return builder.Create(IsLeftHanded);
		}
	}
}