using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		public const int DefaultTileSizeX = 100;
		public const int DefaultTileSizeY = 100;
		public const int DefaultTileVertexCountX = 100;
		public const int DefaultTileVertexCountY = 100;
		public const int DefaultSplatTextureSizeX = 128;
		public const int DefaultSplatTextureSizeY = 128;
		public const int DefaultTilesCountX = 10;
		public const int DefaultTilesCountY = 10;
		public const float DefaultTextureScaleX = 10;
		public const float DefaultTextureScaleY = 10;

		private readonly TerrainTile[,] _tiles;
		private Vector2 _tileSplatTextureScale = new Vector2(DefaultTextureScaleX, DefaultTextureScaleY);

		public Point TileSize { get; }
		public Point TileVertexCount { get; }
		public Point TilesCount { get; }

		public Point TileSplatTextureSize { get; }

		public Point Size => TileSize * TilesCount;

		public Vector2 TileTextureScale
		{
			get => _tileSplatTextureScale;
			set
			{
				if (value == _tileSplatTextureScale)
				{
					return;
				}

				_tileSplatTextureScale = value;

				for (var x = 0; x < TilesCount.X; ++x)
				{
					for (var y = 0; y < TilesCount.Y; ++y)
					{
						_tiles[x, y].InvalidateMesh();
					}
				}
			}
		}

		public float DefaultHeight { get; } = 0.0f;

		public float MaximumHeight { get; set; } = 10.0f;
		public float MinimumHeight { get; set; } = -10.0f;

		[Browsable(false)]
		public TerrainTile this[int x, int y] => _tiles[x, y];

		public Color DiffuseColor { get; set; } = Color.White;

		[Browsable(false)]
		public string TextureBaseName { get; set; }

		[Browsable(false)]
		public string TexturePaintName1 { get; set; }

		[Browsable(false)]
		public string TexturePaintName2 { get; set; }

		[Browsable(false)]
		public string TexturePaintName3 { get; set; }

		[Browsable(false)]
		public string TexturePaintName4 { get; set; }

		public Texture2D TextureBase { get; set; }
		public Texture2D TexturePaint1 { get; set; }
		public Texture2D TexturePaint2 { get; set; }
		public Texture2D TexturePaint3 { get; set; }
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

		public Terrain(int tileSizeX = DefaultTileSizeX, int tileSizeY = DefaultTileSizeY,
			int tileVertexCountX = DefaultTileVertexCountX, int tileVertexCountY = DefaultTileVertexCountY,
			int tilesCountX = DefaultTilesCountX, int tilesCountY = DefaultTilesCountY,
			int tileSplatTextureWidth = DefaultSplatTextureSizeX, int tileSplatTextureHeight = DefaultSplatTextureSizeY,
			float defaultHeight = 0.0f)
		{
			TileSize = new Point(tileSizeX, tileSizeY);
			TileVertexCount = new Point(tileVertexCountX, tileVertexCountY);
			TilesCount = new Point(tilesCountX, tilesCountY);
			TileSplatTextureSize = new Point(tileSplatTextureWidth, tileSplatTextureHeight);
			DefaultHeight = defaultHeight;

			_tiles = new TerrainTile[TilesCount.X, TilesCount.Y];
			for (var x = 0; x < TilesCount.X; ++x)
			{
				for (var y = 0; y < TilesCount.Y; ++y)
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
				return DefaultHeight;
			}

			var tile = GetTileByHeightPosition(heightPos);
			if (tile == null)
			{
				return DefaultHeight;
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

		private TerrainTile GetTileBySplatPosition(Point splatPosition) =>
			SafelyGetTile(splatPosition.X / TileSplatTextureSize.X, splatPosition.Y / TileSplatTextureSize.Y);

		public float GetSplatValue(Point splatPos, SplatManChannel channel)
		{
			if (splatPos.X < 0 || splatPos.Y < 0)
			{
				return 0;
			}

			var tile = GetTileBySplatPosition(splatPos);
			if (tile == null)
			{
				return 0;
			}
			
			var localX = splatPos.X % TileSplatTextureSize.X;
			var localY = splatPos.Y % TileSplatTextureSize.Y;

			var c = tile.GetSplatData(localX, localY);

			byte result = 0;
			switch (channel)
			{
				case SplatManChannel.First:
					result = c.R;
					break;
				case SplatManChannel.Second:
					result = c.G;
					break;
				case SplatManChannel.Third:
					result = c.B;
					break;
				case SplatManChannel.Fourth:
					result = c.A;
					break;
			}

			return (float)result / 255.0f;
		}

		public void SetSplatValue(Point splatPos, SplatManChannel channel, float value)
		{
			if (splatPos.X < 0 || splatPos.Y < 0)
			{
				return;
			}

			var tile = GetTileBySplatPosition(splatPos);
			if (tile == null)
			{
				return;
			}

			var localX = splatPos.X % TileSplatTextureSize.X;
			var localY = splatPos.Y % TileSplatTextureSize.Y;

			var c = tile.GetSplatData(localX, localY);

			byte bvalue = (byte)MathHelper.Clamp(value * 255.0f, 0, 255);
			byte oldValue = 0;
			switch (channel)
			{
				case SplatManChannel.First:
					oldValue = c.R;
					break;
				case SplatManChannel.Second:
					oldValue = c.G;
					break;
				case SplatManChannel.Third:
					oldValue = c.B;
					break;
				case SplatManChannel.Fourth:
					oldValue = c.A;
					break;
			}

			if (oldValue == bvalue)
			{
				return;
			}

			switch (channel)
			{
				case SplatManChannel.First:
					c.R = bvalue;
					break;
				case SplatManChannel.Second:
					c.G = bvalue;
					break;
				case SplatManChannel.Third:
					c.B = bvalue;
					break;
				case SplatManChannel.Fourth:
					c.A = bvalue;
					break;
			}

			// Make sure sum of channels doesnt exceed 1f
			var v = c.ToVector4();
			var sum = v.X + v.Y + v.Z + v.W;
			if (sum > 1f)
			{
				var correction = 1f / sum;
				v.X *= correction;
				v.Y *= correction;
				v.Z *= correction;
				v.W *= correction;

				c = v.ToColor();
			}

			tile.SetSplatData(localX, localY, c);
		}
	}
}