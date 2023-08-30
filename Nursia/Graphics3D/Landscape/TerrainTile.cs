using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Graphics3D.Modelling;
using System.Linq;

namespace Nursia.Graphics3D.Landscape
{
	public class TerrainTile
	{
		private readonly Terrain _terrain;
		private readonly int _tileX, _tileZ;
		private Mesh _mesh = null;
		private MeshPart _meshPart = null;
		private BoundingBox _boundingBox;

		public Texture2D Texture;

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

				var transform = Matrix.CreateTranslation(new Vector3(TileX * Size - _terrain.Size / 2,
					0,
					TileZ * Size - _terrain.Size / 2));

				_meshPart = new MeshPart
				{
					Mesh = _mesh,
					BoundingBox = _boundingBox,
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
				return _tileZ;
			}
		}

		internal TerrainTile(Terrain terrain, int tileX, int tileZ)
		{
			_terrain = terrain;
			_tileX = tileX;
			_tileZ = tileZ;
		}

		public void InvalidateMesh()
		{
			if (_mesh != null)
			{
				_mesh.Dispose();
				_mesh = null;
			}
		}

		private float GetHeight(float x, float z)
		{
			x += _tileX * _terrain.TileSize;
			z += _tileZ * _terrain.TileSize;

/*			if (x < 0)
			{
				x = 0;
			}

			if (x > _terrain.Size)
			{
				x = _terrain.Size;
			}

			if (z < 0)
			{
				z = 0;
			}

			if (z > _terrain.Size)
			{
				z = _terrain.Size;
			}*/

			return _terrain.HeightFunc(x, z);
		}

		private Vector3 CalculateNormal(float x, float z)
		{
			float heightL = GetHeight(x - 1, z);
			float heightR = GetHeight(x + 1, z);
			float heightD = GetHeight(x, z - 1);
			float heightU = GetHeight(x, z + 1);

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

			_boundingBox = BoundingBox.CreateFromPoints(from v in vertices select v.Position);

			_mesh = Mesh.Create(vertices, indices);
		}

		private Mesh GetMesh()
		{
			if (_mesh != null)
			{
				return _mesh;
			}

			if (_terrain.HeightFunc == null)
			{
				CreateFlatTile();
				return _mesh;
			}

			var size = _terrain.TileResolution;
			var vertices = new VertexPositionNormalTexture[size * size];
			var indices = new short[6 * (size - 1) * size];

			var idx = 0;
			for(var z = 0; z < size; ++z)
			{
				for(var x = 0; x < size; ++x)
				{
					var vx = x * Size / (size - 1);
					var vz = z * Size / (size - 1);
					float height = GetHeight(vx, vz);

					var position = new Vector3(vx, height, vz);
					vertices[idx] = new VertexPositionNormalTexture(
						position,
						CalculateNormal(vx, vz),
						new Vector2((float)x / (size - 1), (float)z / (size - 1))
					);

					++idx;
				}
			}

			_boundingBox = BoundingBox.CreateFromPoints(from v in vertices select v.Position);

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
