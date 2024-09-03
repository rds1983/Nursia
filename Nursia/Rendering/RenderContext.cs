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
		public BoundingFrustum Frustrum { get; private set; }

		public RenderStatistics Statistics { get; } = new RenderStatistics();

		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<PointLight> PointLights { get; } = new List<PointLight>();


		internal Dictionary<string, List<RenderJob>> Jobs { get; } = new Dictionary<string, List<RenderJob>>();

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
			Frustrum = new BoundingFrustum(ViewProjection);

			Statistics.Reset();
			Jobs.Clear();
			DirectLights.Clear();
			PointLights.Clear();

			return true;
		}

		public void BatchJob(string techniqueName, Material material,
			Matrix transform, Mesh mesh)
		{
			List<RenderJob> passJobs;

			if (!Jobs.TryGetValue(techniqueName, out passJobs))
			{
				passJobs = new List<RenderJob>();
				Jobs[techniqueName] = passJobs;
			}

			var job = new RenderJob(techniqueName, material, transform, mesh);
			passJobs.Add(job);
		}
	}
}