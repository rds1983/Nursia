using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;

namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		private float _tileSizeX = 100.0f, _tileSizeZ = 100.0f;
		private int _tilesPerX = 10, _tilesPerZ = 10;
		private TerrainTile[,] _tiles;
		private bool _dirty = true;

		public float TileSizeX
		{
			get => _tileSizeX;
			set
			{
				if (_tileSizeX.EpsilonEquals(value))
				{
					return;
				}

				_tileSizeX = value;
				SetDirty();
			}
		}

		public float TileSizeZ
		{
			get => _tileSizeZ;
			set
			{
				if (_tileSizeZ.EpsilonEquals(value))
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


		public TerrainTile this[int x, int z]
		{
			get
			{
				Update();
				return _tiles[x, z];
			}
		}

		public Color DiffuseColor { get; set; } = Color.White;

		public Texture2D TextureBase { get; set; }

		public float TileResolution = 2.0f;

		public Func<float, float, float> HeightFunc;

		private void SetDirty()
		{
			_dirty = true;
		}

		private void Update()
		{
			if (!_dirty) return;

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
