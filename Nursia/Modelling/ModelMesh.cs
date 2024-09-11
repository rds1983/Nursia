using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Rendering.Vertices;
using System;
using System.Collections.Generic;
using VertexPosition = Nursia.Rendering.Vertices.VertexPosition;

namespace Nursia.Modelling
{
	public class ModelMesh : IDisposable
	{
		public Mesh Mesh { get; private set; }

		public VertexBuffer VertexBuffer => Mesh.VertexBuffer;
		public IndexBuffer IndexBuffer => Mesh.IndexBuffer;
		public int PrimitiveCount => Mesh.PrimitiveCount;
		public bool HasNormals => Mesh.HasNormals;
		public BoundingBox BoundingBox => Mesh.BoundingBox;
		public int VertexCount => Mesh.VertexCount;

		public IMaterial Material { get; set; }
		public Matrix Transform = Matrix.Identity;

		public ModelMesh(Mesh meshData, IMaterial material)
		{
			Mesh = meshData ?? throw new ArgumentNullException(nameof(meshData));
			Material = material ?? throw new ArgumentNullException(nameof(material));
		}

		public ModelMesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, IEnumerable<Vector3> positions, IMaterial material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertexBuffer, indexBuffer, positions, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionNormalTexture[] vertices, short[] indices, IMaterial material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionTexture[] vertices, short[] indices, IMaterial material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionNormal[] vertices, short[] indices, IMaterial material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPosition[] vertices, short[] indices, IMaterial material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
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