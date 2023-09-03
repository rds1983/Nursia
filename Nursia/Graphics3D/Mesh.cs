using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Vertices;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Mesh : IDisposable
	{
		public MeshData MeshData { get; private set; }

		public VertexBuffer VertexBuffer => MeshData.VertexBuffer;
		public IndexBuffer IndexBuffer => MeshData.IndexBuffer;
		public int PrimitiveCount => MeshData.PrimitiveCount;
		public bool HasNormals => MeshData.HasNormals;
		public BoundingBox BoundingBox => MeshData.BoundingBox;
		public int VertexCount => MeshData.VertexCount;

		public Material Material { get; set; }
		public Matrix Transform = Matrix.Identity;

		public Mesh(MeshData meshData, Material material)
		{
			MeshData = meshData ?? throw new ArgumentNullException(nameof(meshData));
			Material = material ?? throw new ArgumentNullException(nameof(material));
		}

		public Mesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, IEnumerable<Vector3> positions, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new MeshData(vertexBuffer, indexBuffer, positions, primitiveType), material)
		{
		}

		public Mesh(VertexPositionNormalTexture[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new MeshData(vertices, indices, primitiveType), material)
		{
		}

		public Mesh(VertexPositionTexture[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new MeshData(vertices, indices, primitiveType), material)
		{
		}

		public Mesh(VertexPositionNormal[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new MeshData(vertices, indices, primitiveType), material)
		{
		}

		public Mesh(VertexPosition[] vertices, short[] indices, Material material, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(new MeshData(vertices, indices, primitiveType), material)
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