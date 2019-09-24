namespace Nursia.Graphics3D.Landscape
{
	public class Terrain
	{
		private readonly TerrainTile[,] _tiles;

		public float TileSize { get; set; } = 100;

		public int TilesPerX { get; private set; } = 32;
		public int TilesPerZ { get; private set; } = 32;

		public float TotalSizeX
		{
			get
			{
				return TileSize * TilesPerX; 
			}
		}

		public float TotalSizeZ
		{
			get
			{
				return TileSize * TilesPerZ;
			}
		}

		public TerrainTile this[int x, int z]
		{
			get
			{
				return _tiles[x, z];
			}
		}

		public Terrain(int tilesPerX = 32, int tilesPerZ = 32)
		{
			TilesPerX = tilesPerX;
			TilesPerZ = tilesPerZ;

			_tiles = new TerrainTile[TilesPerX, tilesPerZ];
			for(var x = 0; x < TilesPerX; ++x)
			{
				for(var z = 0; z < TilesPerZ; ++z)
				{
					_tiles[x, z] = new TerrainTile(this, x, z);
				}
			}
		}
	}
}
