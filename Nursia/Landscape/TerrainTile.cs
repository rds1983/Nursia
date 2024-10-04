using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;

namespace Nursia.Landscape
{
	public class TerrainTile
	{
		private readonly Point _pos;
		internal readonly float[] _heightMap;
		internal readonly Color[] _splatData;
		private readonly Matrix _transform;

		private Mesh _meshData = null;
		private Texture2D _splatTexture;

		public Terrain Terrain { get; }

		public Mesh MeshData
		{
			get
			{
				Update();
				return _meshData;
			}
		}

		private Point Size => Terrain.TileSize;

		public Point Position => _pos;

		public Texture2D SplatTexture
		{
			get
			{
				if (_splatTexture == null)
				{
					_splatTexture = new Texture2D(Nrs.GraphicsDevice, Terrain.TileSplatTextureSize.X, Terrain.TileSplatTextureSize.Y);
					_splatTexture.SetData(_splatData);
				}

				return _splatTexture;
			}
		}

		public Matrix Transform => _transform;

		internal TerrainTile(Terrain terrain, int tileX, int tileZ)
		{
			Terrain = terrain;
			_pos = new Point(tileX, tileZ);
			_splatData = new Color[terrain.TileSplatTextureSize.X * terrain.TileSplatTextureSize.Y];
			_heightMap = new float[terrain.TileVertexCount.X * terrain.TileVertexCount.Y];
			_transform = Matrix.CreateTranslation(new Vector3(_pos.X * Size.X, 0, _pos.Y * Size.Y));
		}

		public void InvalidateMesh()
		{
			_meshData?.Dispose();
			_meshData = null;
		}

		private void InvalidateSplatTexture()
		{
			_splatTexture?.Dispose();
			_splatTexture = null;
		}

		public Color GetSplatData(Point localSplatPos)
		{
			var index = localSplatPos.Y * Terrain.TileSplatTextureSize.X + localSplatPos.X;
			return _splatData[index];
		}

		public Color GetSplatData(int x, int y) => GetSplatData(new Point(x, y));

		public void SetSplatData(Point localSplatPos, Color data)
		{
			var index = localSplatPos.Y * Terrain.TileSplatTextureSize.X + localSplatPos.X;
			if (_splatData[index] == data)
			{
				return;
			}

			_splatData[index] = data;
			InvalidateSplatTexture();
		}

		public void SetSplatData(int x, int y, Color data) => SetSplatData(new Point(x, y), data);

		public float GetHeight(Point localHeightPos) => _heightMap[localHeightPos.Y * Terrain.TileVertexCount.X + localHeightPos.X];
		public float GetHeight(int x, int y) => GetHeight(new Point(x, y));

		public void SetHeight(Point localHeightPos, float height)
		{
			height = MathHelper.Clamp(height, Terrain.MinimumHeight, Terrain.MaximumHeight);
			var index = localHeightPos.Y * Terrain.TileVertexCount.X + localHeightPos.X;
			if (_heightMap[index].EpsilonEquals(height))
			{
				return;
			}

			_heightMap[index] = height;
			InvalidateMesh();
		}

		public void SetHeight(int x, int y, float height) => SetHeight(new Point(x, y), height);

		private Vector3 CalculateNormal(Vector2 pos)
		{
			float heightL = Terrain.GetHeight(pos.X - 1, pos.Y);
			float heightR = Terrain.GetHeight(pos.X + 1, pos.Y);
			float heightD = Terrain.GetHeight(pos.X, pos.Y - 1);
			float heightU = Terrain.GetHeight(pos.X, pos.Y + 1);

			var result = new Vector3(heightL - heightR, 2, heightD - heightU);
			result.Normalize();

			return result;
		}


		/// <summary>
		/// Determines whether the tile has any height point that differs from the Terrain default height
		/// </summary>
		/// <returns></returns>
		public bool CheckIfFlatTile()
		{
			var result = true;

			for (var i = 0; i < _heightMap.Length; ++i)
			{
				var h = _heightMap[i];
				if (!h.EpsilonEquals(Terrain.DefaultHeight))
				{
					result = false;
					goto finish;
				}
			}
		finish:;
			return result;
		}

		/// <summary>
		/// Determines whether the tile has any paint textures on it
		/// </summary>
		/// <returns></returns>
		public bool CheckIfCleanTile()
		{
			var result = true;

			for (var i = 0; i < _splatData.Length; ++i)
			{
				var s = _splatData[i];
				if (s != Color.Transparent)
				{
					result = false;
					goto finish;
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
			ushort[] indices;
			if (isFlatTile)
			{
				var h = Terrain.DefaultHeight;
				vertices = new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(new Vector3(0, h, 0), Vector3.Up,  Vector2.Zero),
					new VertexPositionNormalTexture(new Vector3(0, h, Size.Y), Vector3.Up, new Vector2(0, 1.0f)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, 0), Vector3.Up, new Vector2(1.0f, 0)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, 0), Vector3.Up, new Vector2(1.0f, 0)),
					new VertexPositionNormalTexture(new Vector3(0, h, Size.Y), Vector3.Up, new Vector2(0, 1.0f)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, Size.Y), Vector3.Up, new Vector2(1.0f, 1.0f))
				};

				indices = new ushort[]
				{
					0, 1, 2, 3, 4, 5
				};
			}
			else
			{
				var sizeX = Terrain.TileVertexCount.X;
				var sizeY = Terrain.TileVertexCount.Y;
				var idx = 0;
				vertices = new VertexPositionNormalTexture[sizeX * sizeY];
				for (var y = 0; y < sizeY; ++y)
				{
					for (var x = 0; x < sizeX; ++x)
					{
						float vx = x * Size.X / (float)(sizeX - 1);
						float vy = y * Size.Y / (float)(sizeY - 1);

						var globalPos = new Vector2(_pos.X * Terrain.TileSize.X + vx,
													_pos.Y * Terrain.TileSize.X + vy);

						float height = Terrain.GetHeight(globalPos);
						var position = new Vector3(vx, height, vy);
						vertices[idx] = new VertexPositionNormalTexture(position, CalculateNormal(globalPos),
							new Vector2((float)x / (sizeX - 1), (float)y / (sizeY - 1)));

						++idx;
					}
				}

				idx = 0;
				indices = new ushort[6 * (sizeX - 1) * (sizeY - 1)];
				for (var z = 0; z < sizeY - 1; ++z)
				{
					for (var x = 0; x < sizeX - 1; ++x)
					{
						ushort topLeft = (ushort)(z * sizeX + x);
						ushort topRight = (ushort)(topLeft + 1);
						ushort bottomLeft = (ushort)((z + 1) * sizeX + x);
						ushort bottomRight = (ushort)(bottomLeft + 1);

						indices[idx++] = topLeft;
						indices[idx++] = bottomLeft;
						indices[idx++] = topRight;
						indices[idx++] = topRight;
						indices[idx++] = bottomLeft;
						indices[idx++] = bottomRight;
					}
				}
			}

			_meshData = new Mesh(vertices, indices);
		}
	}
}
