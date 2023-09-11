using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;
using System.ComponentModel;

namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		private int _tileSizeX = 100, _tileSizeZ = 100;
		private int _tilesPerX = 10, _tilesPerZ = 10;
		private int _resolutionX = 1, _resolutionZ = 1, _oldResolutionX = 1, _oldResolutionZ = 1;
		private TerrainTile[,] _tiles;
		internal float[,] _heightMap;
		private int _tileSplatTextureWidth = 128, _tileSplatTextureHeight = 128;
		private bool _dirty = true;

		public int TileSizeX
		{
			get => _tileSizeX;
			set
			{
				if (_tileSizeX == value)
				{
					return;
				}

				_tileSizeX = value;
				SetDirty();
			}
		}

		public int TileSizeZ
		{
			get => _tileSizeZ;
			set
			{
				if (_tileSizeZ == value)
				{
					return;
				}

				_tileSizeZ = value;
				SetDirty();
			}
		}

		public int TilesPerX
		{
			get => _tilesPerX;
			set
			{
				if (value == _tilesPerX)
				{
					return;
				}

				_tilesPerX = value;
				SetDirty();
			}
		}

		public int TilesPerZ
		{
			get => _tilesPerZ;
			set
			{
				if (value == _tilesPerZ)
				{
					return;
				}

				_tilesPerZ = value;
				SetDirty();
			}
		}

		public int TileSplatTextureWidth
		{
			get => _tileSplatTextureWidth;

			set
			{
				if (value == _tileSplatTextureWidth)
				{
					return;
				}

				_tileSplatTextureWidth = value;
				SetDirty();
			}
		}

		public int TileSplatTextureHeight
		{
			get => _tileSplatTextureHeight;

			set
			{
				if (value == _tileSplatTextureHeight)
				{
					return;
				}

				_tileSplatTextureHeight = value;
				SetDirty();
			}
		}

		public int ResolutionX
		{
			get => _resolutionX;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				if (value == _resolutionX)
				{
					return;
				}

				_oldResolutionX = _resolutionX;
				_resolutionX = value;
				SetDirty();
			}
		}

		public int ResolutionZ
		{
			get => _resolutionZ;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				if (value == _resolutionZ)
				{
					return;
				}

				_oldResolutionZ = value;
				_resolutionZ = value;
				SetDirty();
			}
		}

		public int SizeX => TileSizeX * TilesPerX;
		public int SizeZ => TileSizeZ * TilesPerZ;

		public float MaximumHeight { get; set; } = 10.0f;
		public float MinimumHeight { get; set; } = -10.0f;


		public TerrainTile this[int x, int z]
		{
			get
			{
				Update();
				return _tiles[x, z];
			}
		}

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


		private void SetDirty()
		{
			_dirty = true;
		}

		public float GetHeight(float x, float z)
		{
			Update();

			var hx = (int)(x * ResolutionX);
			if (hx < 0 || hx >= _heightMap.GetLength(0))
			{
				return 0.0f;
			}

			var hz = (int)(z * ResolutionZ);
			if (hz < 0 || hz >= _heightMap.GetLength(1))
			{
				return 0.0f;
			}

			return _heightMap[hx, hz];
		}

		public TerrainTile GetTile(float x, float z) => _tiles[(int)(x / TileSizeX), (int)(z / TileSizeZ)];

		public void SetHeight(float x, float z, float height)
		{
			Update();

			var hx = (int)(x * ResolutionX);
			if (hx < 0 || hx >= _heightMap.GetLength(0))
			{
				return;
			}

			var hz = (int)(z * ResolutionZ);
			if (hz < 0 || hz >= _heightMap.GetLength(1))
			{
				return;
			}

			if (height < MinimumHeight)
			{
				height = MinimumHeight;
			}

			if (height > MaximumHeight)
			{
				height = MaximumHeight;
			}

			if (height.EpsilonEquals(_heightMap[hx, hz]))
			{
				return;
			}

			_heightMap[hx, hz] = height;

			var tile = GetTile(x, z);
			tile.InvalidateMesh();
		}

		public Point ToSplatPosition(float x, float z) => new Point((int)((x * TileSplatTextureWidth) / TileSizeX),
				(int)((z * TileSplatTextureHeight) / TileSizeZ));

		public Vector2 ToTerrainPosition(Point splatPos) => new Vector2((float)splatPos.X * TileSizeX / TileSplatTextureWidth,
				(float)splatPos.Y * TileSizeZ / TileSplatTextureHeight);

		private TerrainTile GetTileBySplatPosition(Point splatPosition) => _tiles[splatPosition.X / TileSplatTextureWidth, splatPosition.Y / TileSplatTextureHeight];

		public float GetSplatValue(Point splatPos, SplatManChannel channel)
		{
			var tile = GetTileBySplatPosition(splatPos);
			var localX = splatPos.X % TileSplatTextureWidth;
			var localY = splatPos.Y % TileSplatTextureHeight;

			if (localX < 0 || localY < 0)
			{
				return 0;
			}

			var index = localY * TileSplatTextureHeight + localX;
			var c = tile.SplatData[index];

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
			var localX = splatPos.X % TileSplatTextureWidth;
			var localY = splatPos.Y % TileSplatTextureHeight;

			if (localX < 0 || localY < 0)
			{
				return;
			}

			var index = localY * TileSplatTextureHeight + localX;
			var v = tile.SplatData[index];

			float oldValue = 0;
			switch (channel)
			{
				case SplatManChannel.First:
					oldValue = v.X;
					break;
				case SplatManChannel.Second:
					oldValue = v.Y;
					break;
				case SplatManChannel.Third:
					oldValue = v.Z;
					break;
				case SplatManChannel.Fourth:
					oldValue = v.W;
					break;
			}

			if (oldValue.EpsilonEquals(value))
			{
				return;
			}

			switch (channel)
			{
				case SplatManChannel.First:
					v.X = value;
					break;
				case SplatManChannel.Second:
					v.Y = value;
					break;
				case SplatManChannel.Third:
					v.Z = value;
					break;
				case SplatManChannel.Fourth:
					v.W = value;
					break;
			}

			// Make sure sum of channels doesnt exceed 1f
			var sum = v.X + v.Y + v.Z + v.W;
			if (sum > 1f)
			{
				var correction = 1f / sum;
				v.X *= correction;
				v.Y *= correction;
				v.Z *= correction;
				v.W *= correction;
			}

			tile.SplatData[index] = v;
			tile.InvalidateSplatTexture();
		}

		private void Update()
		{
			if (!_dirty) return;

			var oldHeightMap = _heightMap;
			_heightMap = new float[SizeX * ResolutionX, SizeZ * ResolutionZ];
			if (oldHeightMap != null)
			{
				// Keep old heights
				for (var oldHeightX = 0; oldHeightX < _heightMap.GetLength(0); ++oldHeightX)
				{
					for (var oldHeightZ = 0; oldHeightZ < _heightMap.GetLength(1); ++oldHeightZ)
					{
						var x = oldHeightX / _oldResolutionX;
						var z = oldHeightZ / _oldResolutionZ;
						var height = oldHeightMap[oldHeightX, oldHeightZ];
						SetHeight(x, z, height);
					}
				}
			}

			_tiles = new TerrainTile[_tilesPerX, _tilesPerZ];
			for (var x = 0; x < _tilesPerX; ++x)
			{
				for (var z = 0; z < _tilesPerZ; ++z)
				{
					_tiles[x, z] = new TerrainTile(this, x, z);
				}
			}

			_dirty = false;
		}
	}
}