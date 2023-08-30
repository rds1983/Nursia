using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D
{
	public static class PrimitivesFactory
	{
		public static readonly short[] BoxIndices =
{
			0, 1, 3, 1, 2, 3, 1, 5, 2,
			2, 5, 6, 4, 7, 5, 5, 7, 6,
			0, 3, 4, 4, 3, 7, 7, 3, 6,
			6, 3, 2, 4, 5, 0, 0, 5, 1
		};

		public static Vector3[] CreateBox(Vector3 min, Vector3 max)
		{
			return new Vector3[]
			{
				new Vector3(min.X, max.Y, max.Z),
				new Vector3(max.X, max.Y, max.Z),
				new Vector3(max.X, min.Y, max.Z),
				new Vector3(min.X, min.Y, max.Z),
				new Vector3(min.X, max.Y, min.Z),
				new Vector3(max.X, max.Y, min.Z),
				new Vector3(max.X, min.Y, min.Z),
				new Vector3(min.X, min.Y,min.Z)
			};
		}
	}
}
