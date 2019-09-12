using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;
using Nursia.Utilities;
using System;
using System.Linq;

namespace Nursia.Graphics3D.Terrain
{
	public class TerrainTile
	{
		public const float Size = 100;

		private HeightMap _heightMap = null;
		private Mesh _mesh = null;
		private MeshPart _meshPart = null;
		private BoundingSphere _boundingSphere;

		public float X;
		public float Z;

		public Texture2D Texture;

		public HeightMap HeightMap
		{
			get
			{
				return _heightMap;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (_heightMap == value)
				{
					return;
				}

				_heightMap = value;
				InvalidateMesh();
			}
		}

		public MeshPart MeshPart
		{
			get
			{
				if (_meshPart != null)
				{
					return _meshPart;
				}

				_mesh = GetMesh();

				var material = new Material
				{
					DiffuseColor = Color.White,
					Texture = Texture
				};

				_meshPart = new MeshPart
				{
					Mesh = _mesh,
					BoundingSphere = _boundingSphere,
					Material = material
				};

				return _meshPart;
			}
		}

		public TerrainTile(Image2D heightMap)
		{
			HeightMap = new HeightMap(heightMap);
		}

		public void InvalidateMesh()
		{
			if (_mesh != null)
			{
				_mesh.Dispose();
				_mesh = null;
			}
		}

		private Vector3 CalculateNormal(int x, int z)
		{
			float heightL = _heightMap[x - 1, z];
			float heightR = _heightMap[x + 1, z];
			float heightD = _heightMap[x, z - 1];
			float heightU = _heightMap[x, z + 1];

			var result = new Vector3(heightL - heightR, 2, heightD - heightU);
			result.Normalize();

			return result;
		}

		private Mesh GetMesh()
		{
			if (_mesh != null)
			{
				return _mesh;
			}

			var size = _heightMap.Size;
			var vertices = new VertexPositionNormalTexture[size * size];
			var indices = new short[6 * (size - 1) * size];

			var idx = 0;
			for(var z = 0; z < size; ++z)
			{
				for(var x = 0; x < size; ++x)
				{
					float height = _heightMap[x, z];

					var position = new Vector3((x * Size / (size - 1)) -Size / 2,
						height,
						(z * Size / (size - 1)) - Size / 2);
					vertices[idx] = new VertexPositionNormalTexture(
						position,
						CalculateNormal(x, z),
						new Vector2((float)x / (size - 1), (float)z / (size - 1))
					);

					++idx;
				}
			}

			_boundingSphere = BoundingSphere.CreateFromPoints(from v in vertices select v.Position);

			idx = 0;
			for(var z = 0; z < size - 1; ++z)
			{
				for (var x = 0; x < size - 1; ++x)
				{
					short topLeft = (short)((z * size) + x);
					short topRight = (short)(topLeft + 1);
					short bottomLeft = (short)(((z + 1) * size) + x);
					short bottomRight = (short)(bottomLeft + 1);

					indices[idx++] = topLeft;
					indices[idx++] = bottomLeft;
					indices[idx++] = topRight;
					indices[idx++] = topRight;
					indices[idx++] = bottomLeft;
					indices[idx++] = bottomRight;
				}
			}

			return Mesh.Create(vertices, indices);
		}
	}
}
