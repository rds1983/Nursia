using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Rendering.Vertices;
using System;
using System.Collections.Generic;

namespace Nursia.Modelling
{
	public class ModelMesh : IDisposable
	{
		public Mesh MeshData { get; private set; }

		public VertexBuffer VertexBuffer => MeshData.VertexBuffer;
		public IndexBuffer IndexBuffer => MeshData.IndexBuffer;
		public int PrimitiveCount => MeshData.PrimitiveCount;
		public bool HasNormals => MeshData.HasNormals;
		public BoundingBox BoundingBox => MeshData.BoundingBox;
		public int VertexCount => MeshData.VertexCount;

		public Material Material { get; set; }
		public Matrix Transform = Matrix.Identity;

		public ModelMesh(Mesh meshData, Material material)
		{
			MeshData = meshData ?? throw new ArgumentNullException(nameof(meshData));
			Material = material ?? throw new ArgumentNullException(nameof(material));
		}

		public ModelMesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, IEnumerable<Vector3> positions, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertexBuffer, indexBuffer, positions, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionNormalTexture[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionTexture[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPositionNormal[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public ModelMesh(VertexPosition[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new Mesh(vertices, indices, primitiveType), material)
		{
		}

		public void Dispose()
		{
			if (MeshData == null)
			{
				return;
			}

			MeshData.Dispose();
			MeshData = null;
		}
	}
}