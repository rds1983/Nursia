using Microsoft.Xna.Framework;
using Nursia.Rendering.Lights;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public class RenderContext
	{
		private Camera _camera;

		public Camera Camera => _camera;

		public Matrix Projection { get; private set; }
		public Matrix ViewProjection { get; private set; }
		public BoundingFrustum Frustum { get; private set; }

		public RenderStatistics Statistics { get; } = new RenderStatistics();

		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<PointLight> PointLights { get; } = new List<PointLight>();


		internal List<RenderJob> Jobs { get; } = new List<RenderJob>();

		internal bool PrepareRender(Camera camera)
		{
			_camera = camera;
			var device = Nrs.GraphicsDevice;
			if (device.Viewport.Width == 0 || device.Viewport.Height == 0)
			{
				return false;
			}

			// Calculate matrices
			Projection = camera.CalculateProjection();
			ViewProjection = _camera.View * Projection;
			Frustum = new BoundingFrustum(ViewProjection);

			Statistics.Reset();
			Jobs.Clear();
			DirectLights.Clear();
			PointLights.Clear();

			return true;
		}

		public void BatchJob(SceneNode node, IMaterial material, Matrix transform, Mesh mesh)
		{
			var job = new RenderJob(node, material, transform, mesh);

			Jobs.Add(job);
		}
	}
}