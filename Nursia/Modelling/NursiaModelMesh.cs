using Microsoft.Xna.Framework;
using Nursia.Rendering;
using System;

namespace Nursia.Modelling
{
	public class NursiaModelMesh : IDisposable
	{
		public Mesh Mesh { get; private set; }

		public BoundingBox BoundingBox => Mesh.BoundingBox;

		public NursiaModelBone ParentBone { get; internal set; }
		public IMaterial Material { get; set; }
		public Matrix Transform = Matrix.Identity;

		public NursiaModelMesh(Mesh meshData)
		{
			Mesh = meshData ?? throw new ArgumentNullException(nameof(meshData));
		}

		public void Dispose()
		{
			if (Mesh == null)
			{
				return;
			}

			Mesh.Dispose();
			Mesh = null;
		}
	}
}