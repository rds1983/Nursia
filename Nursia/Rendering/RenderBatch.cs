using Microsoft.Xna.Framework;
using Nursia.Rendering.Lights;
using Nursia.Utilities;
using System.Collections.Generic;
using static System.Reflection.Metadata.BlobBuilder;

namespace Nursia.Rendering
{
	public enum RenderBatchPass
	{
		ShadowMap,
		Main
	}

	public class RenderBatch
	{
		private class DefaultComparer : IComparer<RenderJob>
		{
			public static readonly DefaultComparer Instance = new DefaultComparer();
			public int Compare(RenderJob x, RenderJob y)
			{
				if (x.EffectBatchId < y.EffectBatchId)
					return -1;
				if (x.EffectBatchId > y.EffectBatchId)
					return +1;

				return 0;
			}
		}

		private bool _jobsSorted = false;

		public RenderBatchPass Pass { get; private set; }
		public Camera Camera { get; private set; }
		public Matrix View { get; private set; }
		public Matrix Projection { get; private set; }
		public Matrix ViewProjection { get; private set; }
		public BoundingFrustum Frustum { get; private set; }

		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<PointLight> PointLights { get; } = new List<PointLight>();

		private List<RenderJob> UnsortedJobs = new List<RenderJob>();

		internal List<RenderJob> Jobs
		{
			get
			{
				if (!_jobsSorted)
				{
					UnsortedJobs.Sort(DefaultComparer.Instance);
					_jobsSorted = true;
				}

				return UnsortedJobs;
			}
		}

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
			if (Pass == RenderBatchPass.ShadowMap)
			{
				if (!material.CastsShadows)
				{
					return;
				}

				// Switch to shadow map material during shadow map pass
				material = mesh.HasBones ? ShadowMapMaterial.DefaultSkinning : ShadowMapMaterial.Default;
			}

			var boundingBox = mesh.BoundingBox.Transform(ref transform);
			if (Frustum.Contains(boundingBox) == ContainmentType.Disjoint)
			{
				// Cull invisible meshes
				return;
			}

			var job = new RenderJob(material, transform, mesh);
			Jobs.Add(job);

			_jobsSorted = false;
		}
	}
}
