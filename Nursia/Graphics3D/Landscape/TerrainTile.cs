using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.Landscape
{
	public class TerrainTile
	{
		private MeshData _meshData = null;
		private Matrix? _transform;

		public Terrain Terrain { get; }

		public MeshData MeshData
		{
			get
			{
				Update();
				return _meshData;
			}
		}

		private float SizeX => Terrain.TileSizeX;
		private float SizeZ => Terrain.TileSizeZ;

		public int TileX { get; }
		public int TileZ { get; }

		public Matrix Transform
		{
			get
			{
				if (_transform == null)
				{
					_transform = Matrix.CreateTranslation(new Vector3(TileX * SizeX, 0, TileZ * SizeZ));
				}

				return _transform.Value;
			}
		}

		internal TerrainTile(Terrain terrain, int tileX, int tileZ)
		{
			Terrain = terrain;
			TileX = tileX;
			TileZ = tileZ;
		}

		public void InvalidateMesh()
		{
			if (_meshData != null)
			{
				_meshData.Dispose();
				_meshData = null;
			}
		}

		private float GetHeight(float x, float z)
		{
			x += TileX * SizeX;
			z += TileZ * SizeZ;

			return Terrain.HeightFunc(x, z);
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

		private bool CheckIfFlatTile()
		{
			if (Terrain.HeightFunc == null)
			{
				return true;
			}

			var result = true;
			float? height = null;
			var sizeX = (int)(SizeX * Terrain.TileResolution);
			var sizeZ = (int)(SizeZ * Terrain.TileResolution);

			for (var z = 0; z < sizeZ; ++z)
			{
				for (var x = 0; x < sizeX; ++x)
				{
					var vx = x * SizeX / (sizeX - 1);
					var vz = z * SizeZ / (sizeZ - 1);

					var h = GetHeight(vx, vz);
					if (height == null)
					{
						height = h;
					}
					else if (height.Value != h)
					{
						result = false;
						goto finish;
					}
				}
			}
		finish:;

			return result;
		}

		private void Update()
		{
			if (_meshData != null)
			{
				return;
			}

			var isFlatTile = CheckIfFlatTile();
			VertexPositionNormalTexture[] vertices;
			short[] indices;
			if (isFlatTile)
			{
				vertices = new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Up,  Vector2.Zero),
					new VertexPositionNormalTexture(new Vector3(0, 0, SizeZ), Vector3.Up, new Vector2(0, 1)),
					new VertexPositionNormalTexture(new Vector3(SizeX, 0, 0), Vector3.Up, new Vector2(1, 0)),
					new VertexPositionNormalTexture(new Vector3(SizeX, 0, 0), Vector3.Up, new Vector2(1, 0)),
					new VertexPositionNormalTexture(new Vector3(0, 0, SizeZ), Vector3.Up, new Vector2(0, 1)),
					new VertexPositionNormalTexture(new Vector3(SizeX, 0, SizeZ), Vector3.Up, new Vector2(1, 1))
				};

				indices = new short[]
				{
					0, 1, 2, 3, 4, 5
				};
			}
			else
			{

				var sizeX = (int)(SizeX * Terrain.TileResolution);
				var sizeZ = (int)(SizeZ * Terrain.TileResolution);
				var idx = 0;
				vertices = new VertexPositionNormalTexture[sizeX * sizeZ];
				for (var z = 0; z < sizeZ; ++z)
				{
					for (var x = 0; x < sizeX; ++x)
					{
						var vx = x * SizeX / (sizeX - 1);
						var vz = z * SizeZ / (sizeZ - 1);
						float height = GetHeight(vx, vz);

						var position = new Vector3(vx, height, vz);
						vertices[idx] = new VertexPositionNormalTexture(
							position,
							CalculateNormal(vx, vz),
							new Vector2((float)x / (sizeX - 1), (float)z / (sizeZ - 1))
						);

						++idx;
					}
				}

				idx = 0;
				indices = new short[6 * (sizeX - 1) * (sizeZ - 1)];
				for (var z = 0; z < sizeZ - 1; ++z)
				{
					for (var x = 0; x < sizeX - 1; ++x)
					{
						short topLeft = (short)((z * sizeX) + x);
						short topRight = (short)(topLeft + 1);
						short bottomLeft = (short)(((z + 1) * sizeX) + x);
						short bottomRight = (short)(bottomLeft + 1);

						indices[idx++] = topLeft;
						indices[idx++] = bottomLeft;
						indices[idx++] = topRight;
						indices[idx++] = topRight;
						indices[idx++] = bottomLeft;
						indices[idx++] = bottomRight;
					}
				}
			}

			_meshData = new MeshData(vertices, indices);
		}
	}
}
