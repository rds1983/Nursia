namespace Nursia.Graphics3D
{
	public class RenderStatistics
	{
		public int MeshesDrawn { get; internal set; }

		public void Reset()
		{
			MeshesDrawn = 0;
		}
	}
}
