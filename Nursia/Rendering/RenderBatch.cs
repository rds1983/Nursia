using Microsoft.Xna.Framework;
using Nursia.Rendering.Lights;
using Nursia.Utilities;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public enum RenderBatchPass
	{
		ShadowMap,
		Main
	}

	public class RenderBatch
	{
		public RenderBatchPass Pass { get; private set; }
		public Camera Camera { get; private set; }
		public Matrix View { get; private set; }
		public Matrix Projection { get; private set; }
		public Matrix ViewProjection { get; private set; }
		public BoundingFrustum Frustum { get; private set; }

		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<PointLight> PointLights { get; } = new List<PointLight>();


		internal List<RenderJob> Jobs { get; } = new List<RenderJob>();

		internal void PrepareRender(RenderBatchPass pass, Camera camera)
		{
			Pass = pass;
			View = camera.View;
			Projection = camera.CalculateProjection();
			ViewProjection = View * Projection;
			Frustum = new BoundingFrustum(ViewProjection);

			Jobs.Clear();
			DirectLights.Clear();
			PointLights.Clear();
		}

		public void BatchJob(IMaterial material, Matrix transform, Mesh mesh)
		{
			if (Pass == RenderBatchPass.ShadowMap && !material.CastsShadows)
			{
				return;
			}

			var boundingBox = mesh.BoundingBox.Transform(ref transform);
			if (Frustum.Contains(boundingBox) == ContainmentType.Disjoint)
			{
				// Cull invisible meshes
				return;
			}


			var job = new RenderJob(material, transform, mesh);

			Jobs.Add(job);
		}
	}
}
