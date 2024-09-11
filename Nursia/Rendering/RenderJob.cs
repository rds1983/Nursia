using Microsoft.Xna.Framework;
using Nursia.Utilities;
using System;

namespace Nursia.Rendering
{
	internal class RenderJob
	{
		public SceneNode SceneNode { get; }
		public IMaterial Material { get; }
		public Matrix Transform { get; }
		public Mesh Mesh { get; }

		public RenderJob(SceneNode node, IMaterial material, Matrix transform, Mesh mesh)
		{
			SceneNode = node ?? throw new ArgumentNullException(nameof(node));
			Material = material ?? throw new ArgumentNullException(nameof(material));
			Transform = transform;
			Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}
	}
}
