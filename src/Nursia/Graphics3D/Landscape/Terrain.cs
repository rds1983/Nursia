using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		private readonly TerrainTile[,] _tiles;

		public float Size { get; set; } = 400;

		public float TileSize { get; set; } = 100;

		public float Resolution { get; private set; } = 2.56f;

		internal int TileResolution
		{
			get
			{
				return (int)(TileSize * Resolution);
			}
		}

		public int TilesPerX
		{
			get
			{
				return (int)(Size / TileSize);
			}
		}

		public int TilesPerZ
		{
			get
			{
				return (int)(Size / TileSize);
			}
		}

		public TerrainTile this[int x, int z]
		{
			get
			{
				return _tiles[x, z];
			}
		}

		public Func<float, float, float> HeightFunc;

		public Terrain(float size)
		{
			Size = size;

			int tilesPerX = (int)(Size / TileSize);
			int tilesPerZ = (int)(Size / TileSize);

			_tiles = new TerrainTile[tilesPerX, tilesPerZ];
			for(var x = 0; x < tilesPerX; ++x)
			{
				for(var z = 0; z < tilesPerZ; ++z)
				{
					_tiles[x, z] = new TerrainTile(this, x, z);
				}
			}
		}

		public void SetTexture(Texture2D texture)
		{
			for(var x = 0; x < _tiles.GetLength(0); ++x)
			{
				for(var z = 0; z < _tiles.GetLength(1); ++z)
				{
					_tiles[x, z].Texture = texture;
				}
			}
		}
	}
}
