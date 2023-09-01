using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Vertices;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Mesh : IDisposable
	{
		private MeshData _meshData;

		public MeshData MeshData => _meshData;

		public VertexBuffer VertexBuffer => _meshData.VertexBuffer;
		public IndexBuffer IndexBuffer => _meshData.IndexBuffer;
		public int PrimitiveCount => _meshData.PrimitiveCount;
		public bool HasNormals => _meshData.HasNormals;
		public BoundingBox BoundingBox => _meshData.BoundingBox;
		public int VertexCount => _meshData.VertexCount;

		public Material Material { get; set; }
		public Matrix Transform = Matrix.Identity;

		public Mesh(MeshData meshData, Material material)
		{
			_meshData = meshData ?? throw new ArgumentNullException(nameof(meshData));
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
			if (_meshData == null)
			{
				return;
			}

			_meshData.Dispose();
			_meshData = null;
		}
	}
}