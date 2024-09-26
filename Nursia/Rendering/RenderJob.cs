using Microsoft.Xna.Framework;
using System;

namespace Nursia.Rendering
{
	internal class RenderJob
	{
		public int EffectBatchId => Material.EffectBinding.BatchId;
		public IMaterial Material { get; }
		public Matrix Transform { get; }
		public Mesh Mesh { get; }

		public RenderJob(IMaterial material, Matrix transform, Mesh mesh)
		{
			Material = material ?? throw new ArgumentNullException(nameof(material));
			Transform = transform;
			Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}
	}
}
