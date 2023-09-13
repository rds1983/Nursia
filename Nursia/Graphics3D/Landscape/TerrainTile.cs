using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia.Graphics3D.Landscape
{
	public class TerrainTile
	{
		private readonly Point _pos;
		private readonly float[,] _heightMap;
		private readonly Vector4[] _splatData;
		private readonly Matrix _transform;

		private MeshData _meshData = null;
		private Texture2D _splatTexture;

		public Terrain Terrain { get; }

		public MeshData MeshData
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

					var colors = new Color[_splatData.Length];
					for (var i = 0; i < colors.Length; ++i)
					{
						colors[i] = _splatData[i].ToColor();
					}

					_splatTexture.SetData(colors);
				}

				return _splatTexture;
			}
		}

		public Matrix Transform => _transform;

		internal TerrainTile(Terrain terrain, int tileX, int tileZ)
		{
			Terrain = terrain;
			_pos = new Point(tileX, tileZ);
			_splatData = new Vector4[terrain.TileSplatTextureSize.X * terrain.TileSplatTextureSize.Y];
			_heightMap = new float[terrain.TileVertexCount.X, terrain.TileVertexCount.Y];
			_transform = Matrix.CreateTranslation(new Vector3(_pos.X * Size.X, 0, _pos.Y * Size.Y));
		}

		private void InvalidateMesh()
		{
			_meshData?.Dispose();
			_meshData = null;
		}

		private void InvalidateSplatTexture()
		{
			_splatTexture?.Dispose();
			_splatTexture = null;
		}

		public Vector4 GetSplatData(Point localSplatPos)
		{
			var index = localSplatPos.Y * Terrain.TileSplatTextureSize.X + localSplatPos.X;
			return _splatData[index];
		}

		public Vector4 GetSplatData(int x, int y) => GetSplatData(new Point(x, y));

		public void SetSplatData(Point localSplatPos, Vector4 data)
		{
			var index = localSplatPos.Y * Terrain.TileSplatTextureSize.X + localSplatPos.X;
			if (_splatData[index] == data)
			{
				return;
			}

			_splatData[index] = data;
			InvalidateSplatTexture();
		}

		public void SetSplatData(int x, int y, Vector4 data) => SetSplatData(new Point(x, y), data);

		public float GetHeight(Point localHeightPos) => _heightMap[localHeightPos.X, localHeightPos.Y];
		public float GetHeight(int x, int y) => GetHeight(new Point(x, y));

		public void SetHeight(Point localHeightPos, float height)
		{
			if (height < Terrain.MinimumHeight || height > Terrain.MaximumHeight)
			{
				return;
			}

			if (_heightMap[localHeightPos.X, localHeightPos.Y].EpsilonEquals(height))
			{
				return;
			}

			_heightMap[localHeightPos.X, localHeightPos.Y] = height;
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

		private Vector3 CalculateNormal(float x, float y) => CalculateNormal(new Vector2(x, y));

		private bool CheckIfFlatTile(out float h2)
		{
			var result = true;
			float? height = null;

			for (var x = 0; x < _heightMap.GetLength(0); ++x)
			{
				for (var y = 0; y < _heightMap.GetLength(1); ++y)
				{
					var h = _heightMap[x, y];
					if (height == null)
					{
						height = h;
					}
					else if (!height.Value.EpsilonEquals(h))
					{
						result = false;
						goto finish;
					}
				}
			}
		finish:;

			h2 = height ?? 0;
			return result;
		}

		private void Update()
		{
			if (_meshData != null)
			{
				return;
			}

			float h;
			var isFlatTile = CheckIfFlatTile(out h);
			VertexPositionNormalTexture[] vertices;
			short[] indices;
			if (isFlatTile)
			{
				vertices = new VertexPositionNormalTexture[]
				{
					new VertexPositionNormalTexture(new Vector3(0, h, 0), Vector3.Up,  Vector2.Zero),
					new VertexPositionNormalTexture(new Vector3(0, h, Size.Y), Vector3.Up, new Vector2(0, 1)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, 0), Vector3.Up, new Vector2(1, 0)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, 0), Vector3.Up, new Vector2(1, 0)),
					new VertexPositionNormalTexture(new Vector3(0, h, Size.Y), Vector3.Up, new Vector2(0, 1)),
					new VertexPositionNormalTexture(new Vector3(Size.X, h, Size.Y), Vector3.Up, new Vector2(1, 1))
				};

				indices = new short[]
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
						float vx = x * Terrain.TileSize.X / (float)(sizeX - 1);
						float vy = y * Terrain.TileSize.Y / (float)(sizeY - 1);

						var globalPos = new Vector2(_pos.X * Terrain.TileSize.X + vx, _pos.Y * Terrain.TileSize.X + vy);

						float height = Terrain.GetHeight(globalPos);
						var position = new Vector3(vx, height, vy);
						vertices[idx] = new VertexPositionNormalTexture(
							position,
							CalculateNormal(globalPos),
							new Vector2((float)x / (sizeX - 1), (float)y / (sizeY - 1))
						);

						++idx;
					}
				}

				idx = 0;
				indices = new short[6 * (sizeX - 1) * (sizeY - 1)];
				for (var z = 0; z < sizeY - 1; ++z)
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
