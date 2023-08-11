using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class MeshPart
	{
		public Material Material { get; set; }
		public Mesh Mesh { get; set; }

		public BoundingSphere BoundingSphere { get; set; }

		public Matrix Transform;

		public int? VertexCount = null;
		public int StartIndex = 0;
		public int? PrimitiveCount = null;

		public MeshPart()
		{
			Transform = Matrix.Identity;
		}
	}
}
