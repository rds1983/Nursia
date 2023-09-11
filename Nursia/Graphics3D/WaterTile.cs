using System.ComponentModel;

namespace Nursia.Graphics3D
{
	public class WaterTile
	{
		[Category("Position")]
		public float X { get; set; }

		[Category("Position")]
		public float Z { get; set; }

		[Category("Position")]
		public float Height { get; set; }

		[Category("Position")]
		public float SizeX { get; set; }

		[Category("Position")]
		public float SizeZ { get; set; }

		[Category("Water Parameters")]
		public float Tiling { get; set; } = 4.0f;

		public WaterTile(float x, float z, float height, float sizeX = 40.0f, float sizeZ = 40.0f)
		{
			X = x;
			Z = z;
			Height = height;
			SizeX = sizeX;
			SizeZ = sizeZ;
		}
	}
}
