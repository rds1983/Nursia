namespace Nursia.Graphics3D.Landscape
{
	public class HeightMap
	{
		private float[,] _heights;

		public int Size
		{
			get
			{
				return _heights.GetLength(0);
			}
		}

		public float this[int x, int z]
		{
			get
			{
				if (x < 0 || z < 0 || x >= Size || z >= Size)
				{
					return 0;
				}

				return _heights[x, z];
			}
		}

		public HeightMap(float[,] heights)
		{
			_heights = heights;
		}
	}
}
