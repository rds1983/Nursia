using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		private readonly Point _tileSize;
		private readonly Point _tileVertexCount;
		private readonly Point _tileSplatTextureSize;
		private readonly Point _tilesCount;
		private Point _tileSplatTextureScale = new Point(10, 10);
		private TerrainTile[,] _tiles;

		public Point TileSize => _tileSize;
		public Point TileVertexCount => _tileVertexCount;
		public Point TilesCount => _tilesCount;

		public Point TileSplatTextureSize => _tileSplatTextureSize;

		public Point Size => TileSize * TilesCount;

		public Point TileSplatTextureScale
		{
			get => _tileSplatTextureScale;
			set
			{
				if (value == _tileSplatTextureScale)
				{
					return;
				}

				_tileSplatTextureScale = value;

				for (var x = 0; x < _tilesCount.X; ++x)
				{
					for (var y = 0; y < _tilesCount.Y; ++y)
					{
						_tiles[x, y].InvalidateMesh();
					}
				}
			}
		}

		public float MaximumHeight { get; set; } = 10.0f;
		public float MinimumHeight { get; set; } = -10.0f;

		[Browsable(false)]
		public TerrainTile this[int x, int y] => _tiles[x, y];

		public Color DiffuseColor { get; set; } = Color.White;

		[Browsable(false)]
		public Texture2D TextureBase { get; set; }

		[Browsable(false)]
		public string TexturePaintName1 { get; set; }

		[Browsable(false)]
		public Texture2D TexturePaint1 { get; set; }

		[Browsable(false)]
		public string TexturePaintName2 { get; set; }

		[Browsable(false)]
		public Texture2D TexturePaint2 { get; set; }

		[Browsable(false)]
		public string TexturePaintName3 { get; set; }

		[Browsable(false)]
		public Texture2D TexturePaint3 { get; set; }

		[Browsable(false)]
		public string TexturePaintName4 { get; set; }

		[Browsable(false)]
		public Texture2D TexturePaint4 { get; set; }

		[Browsable(false)]
		public int TexturesCount
		{
			get
			{
				var result = 0;
				if (TextureBase != null)
				{
					++result;
				}

				if (TexturePaint1 != null)
				{
					++result;
				}
				if (TexturePaint2 != null)
				{
					++result;
				}
				if (TexturePaint3 != null)
				{
					++result;
				}
				if (TexturePaint4 != null)
				{
					++result;
				}

				return result;
			}
		}

		public Terrain(int tileSizeX = 100, int tileSizeZ = 100, int tileVertexCountX = 100, int tileVertexCountZ = 100,
			int tilesCountX = 10, int tilesCountZ = 10, int tileSplatTextureWidth = 128, int tileSplatTextureHeight = 128)
		{
			_tileSize = new Point(tileSizeX, tileSizeZ);
			_tileVertexCount = new Point(tileVertexCountX, tileVertexCountZ);
			_tilesCount = new Point(tilesCountX, tilesCountZ);
			_tileSplatTextureSize = new Point(tileSplatTextureWidth, tileSplatTextureHeight);

			_tiles = new TerrainTile[_tilesCount.X, _tilesCount.Y];
			for (var x = 0; x < _tilesCount.X; ++x)
			{
				for (var y = 0; y < _tilesCount.Y; ++y)
				{
					_tiles[x, y] = new TerrainTile(this, x, y);
				}
			}
		}

		private TerrainTile SafelyGetTile(int x, int y)
		{
			if (x < 0 || x >= _tiles.GetLength(0))
			{
				return null;
			}

			if (y < 0 || y >= _tiles.GetLength(1))
			{
				return null;
			}

			return _tiles[x, y];
		}

		public Point ToHeightPosition(Vector2 pos) => new Point((int)(pos.X * TileVertexCount.X / TileSize.X),
				(int)(pos.Y * TileVertexCount.Y / TileSize.Y));

		public Point ToHeightPosition(float x, float y) => ToHeightPosition(new Vector2(x, y));

		public Vector2 HeightToTerrainPosition(Point heightPos) => new Vector2((float)heightPos.X * TileSize.X / TileVertexCount.X,
				(float)heightPos.Y * TileSize.Y / TileVertexCount.Y);

		private TerrainTile GetTileByHeightPosition(Point heightPos) =>
			SafelyGetTile(heightPos.X / TileVertexCount.X, heightPos.Y / TileVertexCount.Y);

		public float GetHeightByHeightPos(Point heightPos)
		{
			if (heightPos.X < 0 || heightPos.Y < 0)
			{
				return 0.0f;
			}

			var tile = GetTileByHeightPosition(heightPos);
			if (tile == null)
			{
				return 0.0f;
			}

			var x = heightPos.X % TileVertexCount.X;
			var y = heightPos.Y % TileVertexCount.Y;
			return tile.GetHeight(x, y);
		}

		public float GetHeightByHeightPos(int x, int y) => GetHeightByHeightPos(new Point(x, y));

		public float GetHeight(Vector2 pos) => GetHeightByHeightPos(ToHeightPosition(pos));

		public float GetHeight(float x, float y) => GetHeight(new Vector2(x, y));

		public void SetHeightByHeightPos(Point heightPos, float height)
		{
			if (heightPos.X < 0 || heightPos.Y < 0)
			{
				return;
			}

			var tile = GetTileByHeightPosition(heightPos);
			if (tile == null)
			{
				return;
			}

			var x = heightPos.X % TileVertexCount.X;
			var y = heightPos.Y % TileVertexCount.Y;
			tile.SetHeight(x, y, height);
		}

		public void SetHeightByHeightPos(int x, int y, float height) => SetHeightByHeightPos(new Point(x, y), height);

		public Point ToSplatPosition(float x, float z) => new Point((int)((x * TileSplatTextureSize.X) / TileSize.X),
				(int)((z * TileSplatTextureSize.Y) / TileSize.Y));

		public Vector2 SplatToTerrainPosition(Point splatPos) => new Vector2((float)splatPos.X * TileSize.X / TileSplatTextureSize.X,
				(float)splatPos.Y * TileSize.Y / TileSplatTextureSize.Y);

		private TerrainTile GetTileBySplatPosition(Point splatPosition) => _tiles[splatPosition.X / TileSplatTextureSize.X, splatPosition.Y / TileSplatTextureSize.Y];

		public float GetSplatValue(Point splatPos, SplatManChannel channel)
		{
			var tile = GetTileBySplatPosition(splatPos);
			var localX = splatPos.X % TileSplatTextureSize.X;
			var localY = splatPos.Y % TileSplatTextureSize.Y;

			if (localX < 0 || localY < 0)
			{
				return 0;
			}

			var c = tile.GetSplatData(localX, localY);

			float result = 0;
			switch (channel)
			{
				case SplatManChannel.First:
					result = c.X;
					break;
				case SplatManChannel.Second:
					result = c.Y;
					break;
				case SplatManChannel.Third:
					result = c.Z;
					break;
				case SplatManChannel.Fourth:
					result = c.W;
					break;
			}

			return result;
		}

		public void SetSplatValue(Point splatPos, SplatManChannel channel, float value)
		{
			var tile = GetTileBySplatPosition(splatPos);
			var localX = splatPos.X % TileSplatTextureSize.X;
			var localY = splatPos.Y % TileSplatTextureSize.Y;

			if (localX < 0 || localY < 0)
			{
				return;
			}

			var c = tile.GetSplatData(localX, localY);

			float oldValue = 0;
			switch (channel)
			{
				case SplatManChannel.First:
					oldValue = c.X;
					break;
				case SplatManChannel.Second:
					oldValue = c.Y;
					break;
				case SplatManChannel.Third:
					oldValue = c.Z;
					break;
				case SplatManChannel.Fourth:
					oldValue = c.W;
					break;
			}

			if (oldValue.EpsilonEquals(value))
			{
				return;
			}

			switch (channel)
			{
				case SplatManChannel.First:
					c.X = value;
					break;
				case SplatManChannel.Second:
					c.Y = value;
					break;
				case SplatManChannel.Third:
					c.Z = value;
					break;
				case SplatManChannel.Fourth:
					c.W = value;
					break;
			}

			// Make sure sum of channels doesnt exceed 1f
			var sum = c.X + c.Y + c.Z + c.W;
			if (sum > 1f)
			{
				var correction = 1f / sum;
				c.X *= correction;
				c.Y *= correction;
				c.Z *= correction;
				c.W *= correction;
			}

			tile.SetSplatData(localX, localY, c);
		}
	}
}