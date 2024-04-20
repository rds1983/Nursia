using Microsoft.Xna.Framework;
using Nursia.Rendering;
using System;

namespace Nursia.Rendering
{
	internal class RenderJob
	{
		public string TechniqueName { get; }
		public Material Material { get; }
		public Matrix Transform { get; }
		public Mesh Mesh { get; }

		public RenderJob(string techniqueName,
			Material material,
			Matrix transform,
			Mesh mesh)
		{
			TechniqueName = techniqueName;
			Material = material ?? throw new ArgumentNullException(nameof(material));
			Transform = transform;
			Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}
	}
}
