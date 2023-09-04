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

			if (height.EpsilonEquals(_heightMap[hx, hz]))
			{
				return;
			}

			_heightMap[hx, hz] = height;

			var tile = _tiles[(int)(x / TileSizeX), (int)(z / TileSizeZ)];
			tile.InvalidateMesh();
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
