namespace Nursia.Graphics3D
{
	public class WaterTile
	{
		public float X { get; private set; }
		public float Z { get; private set; }
		public float Height { get; private set; }
		public float Size { get; private set; }

		public WaterTile(float x, float z, float height, float size = 40.0f)
		{
			X = x;
			Z = z;
			Height = height;
			Size = size;
		}
	}
}
