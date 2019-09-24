using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;
using System;
using System.Linq;

namespace Nursia.Graphics3D.Landscape
{
	public class TerrainTile
	{
		private readonly Terrain _terrain;
		private readonly int _tileX, _tileY;
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

				var transform = Matrix.CreateTranslation(new Vector3(TileX * Size - _terrain.TotalSizeX / 2,
					0,
					TileZ * Size - _terrain.TotalSizeZ / 2));

				_meshPart = new MeshPart
				{
					Mesh = _mesh,
					BoundingSphere = _boundingSphere.Transform(transform),
					Material = material,
					Transform = transform
				};

				return _meshPart;
			}
		}

		private float Size
		{
			get
			{
				return _terrain.TileSize;
			}
		}

		public int TileX
		{
			get
			{
				return _tileX;
			}
		}

		public int TileZ
		{
			get
			{
				return _tileY;
			}
		}

		internal TerrainTile(Terrain terrain, int tileX, int tileZ)
		{
			_terrain = terrain;
			_tileX = tileX;
			_tileY = tileZ;
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

		private void CreateFlatTile()
		{
			var size = Size;
			var vertices = new VertexPositionNormalTexture[]
			{
				new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Up,  Vector2.Zero),
				new VertexPositionNormalTexture(new Vector3(0, 0, size), Vector3.Up, new Vector2(0, 1)),
				new VertexPositionNormalTexture(new Vector3(size, 0, 0), Vector3.Up, new Vector2(1, 0)),
				new VertexPositionNormalTexture(new Vector3(size, 0, 0), Vector3.Up, new Vector2(1, 0)),
				new VertexPositionNormalTexture(new Vector3(0, 0, size), Vector3.Up, new Vector2(0, 1)),
				new VertexPositionNormalTexture(new Vector3(size, 0, size), Vector3.Up, new Vector2(1, 1))
			};

			var indices = new short[]
			{
				0, 1, 2, 3, 4, 5
			};

			_boundingSphere = BoundingSphere.CreateFromPoints(from v in vertices select v.Position);

			_mesh = Mesh.Create(vertices, indices);
		}

		private Mesh GetMesh()
		{
			if (_mesh != null)
			{
				return _mesh;
			}

			if (_heightMap == null)
			{
				CreateFlatTile();
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

					var position = new Vector3((x * Size / (size - 1)),
						height,
						(z * Size / (size - 1)));
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
