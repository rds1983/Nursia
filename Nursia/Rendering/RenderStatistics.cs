namespace Nursia.Rendering
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
