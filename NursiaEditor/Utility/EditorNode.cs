using Microsoft.Xna.Framework;
using Nursia.Rendering;
using System;

namespace NursiaEditor.Utility
{
	internal class EditorNode : SceneNode
	{
		public Mesh Mesh { get; }
		public IMaterial Material { get; }
		public Matrix Transform { get; set; }

		public EditorNode(Mesh mesh, IMaterial material)
		{
			Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
			Material = material ?? throw new ArgumentNullException(nameof(material));
		}

		protected override void Render(RenderBatch batch)
		{
			base.Render(batch);

			batch.BatchJob(Material, Transform, Mesh);
		}
	}
}
