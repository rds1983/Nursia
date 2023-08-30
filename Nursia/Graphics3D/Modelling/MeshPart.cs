using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Graphics3D.Modelling
{
	public class MeshPart
	{
		private BoundingBox _boundingBox;

		public Material Material { get; set; }
		public Mesh Mesh { get; set; }
		public Mesh BoundingBoxMesh { get; private set; }

		public Matrix Transform;

		public int? VertexCount = null;
		public int StartIndex = 0;
		public int? PrimitiveCount = null;

		public BoundingBox BoundingBox
		{
			get => _boundingBox;
			set
			{
				if (_boundingBox == value)
				{
					return;
				}

				_boundingBox = value;

				var points = PrimitivesFactory.CreateBox(value.Min, value.Max);
				var verticesList = new List<VertexPositionColor>();
				for (var i = 0; i < points.Length; i++)
				{
					verticesList.Add(new VertexPositionColor(points[i], Color.White));
				}

				BoundingBoxMesh = Mesh.Create(verticesList.ToArray(), PrimitivesFactory.BoxIndices);
			}
		}

		public MeshPart()
		{
			Transform = Matrix.Identity;
		}
	}
}
