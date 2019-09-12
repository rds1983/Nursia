namespace Nursia.Graphics3D
{
	public class WaterTile
	{
		public const float Size = 40.0f;

		public float X { get; private set; }
		public float Z { get; private set; }
		public float Height { get; private set; }

		public WaterTile(float x, float z, float height)
		{
			X = x;
			Z = z;
			Height = height;
		}
	}
}
