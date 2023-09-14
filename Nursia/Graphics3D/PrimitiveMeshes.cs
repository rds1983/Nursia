using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Vertices;

namespace Nursia.Graphics3D
{
	public static class PrimitiveMeshes
	{
		private static readonly short[] _cubeIndices =
		{
			0, 1, 3, 1, 2, 3, 1, 5, 2,
			2, 5, 6, 4, 7, 5, 5, 7, 6,
			0, 3, 4, 4, 3, 7, 7, 3, 6,
			6, 3, 2, 4, 5, 0, 0, 5, 1
		};

		private static readonly short[] _squareIndices =
		{
			0, 1, 2, 3, 4, 5
		};

		private static Vector3[] _cubeFromMinusOneToOne = new Vector3[]
		{
			new Vector3(-1, 1, 1),
			new Vector3(1, 1, 1),
			new Vector3(1, -1, 1),
			new Vector3(-1, -1, 1),
			new Vector3(-1, 1, -1),
			new Vector3(1, 1, -1),
			new Vector3(1, -1, -1),
			new Vector3(-1, -1,-1)
		};

		private static Vector3[] _cubeFromZeroToOne = new Vector3[]
		{
			new Vector3(0, 1, 1),
			new Vector3(1, 1, 1),
			new Vector3(1, 0, 1),
			new Vector3(0, 0, 1),
			new Vector3(0, 1, 0),
			new Vector3(1, 1, 0),
			new Vector3(1, 0, 0),
			new Vector3(0, 0, 0)
		};

		private static Vector3[] _squareFromMinusOneToOne = new Vector3[]
		{
				new Vector3(-1, 0, -1),
				new Vector3(-1, 0, 1),
				new Vector3(1, 0, -1),
				new Vector3(1, 0, -1),
				new Vector3(-1, 0, 1),
				new Vector3(1, 0, 1)
		};

		private static Vector3[] _squareFromZeroToOne = new Vector3[]
		{
				new Vector3(0, 0, 0),
				new Vector3(0, 0, 1),
				new Vector3(1, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(0, 0, 1),
				new Vector3(1, 0, 1)
		};

		private static VertexPositionTexture[] _squarePositionTextureFromZeroToOneData = new VertexPositionTexture[]
		{
				new VertexPositionTexture(new Vector3(0, 0, 0), Vector2.Zero),
				new VertexPositionTexture(new Vector3(0, 0, 1), new Vector2(0, 1)),
				new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(1, 0, 0),new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(0, 0, 1),new Vector2(0, 1)),
				new VertexPositionTexture(new Vector3(1, 0, 1), new Vector2(1, 1))
		};

		private static MeshData _cubePosition;
		private static MeshData _squarePositionFromZeroToOne;
		private static MeshData _squarePositionTextureFromZeroToOne;

		public static MeshData CubePositionFromMinusOneToOne
		{
			get
			{
				if (_cubePosition == null)
				{
					_cubePosition = CreatePrimitivePosition(_cubeFromMinusOneToOne, _cubeIndices);
				}

				return _cubePosition;
			}
		}

		public static MeshData CubePositionFromZeroToOne
		{
			get
			{
				if (_cubePosition == null)
				{
					_cubePosition = CreatePrimitivePosition(_cubeFromZeroToOne, _cubeIndices);
				}

				return _cubePosition;
			}
		}

		public static MeshData SquarePositionFromMinusOneToOne
		{
			get
			{
				if (_squarePositionTextureFromZeroToOne == null)
				{
					_squarePositionTextureFromZeroToOne = CreatePrimitivePosition(_squareFromMinusOneToOne, _squareIndices);
				}

				return _squarePositionTextureFromZeroToOne;
			}
		}

		public static MeshData SquarePositionFromZeroToOne
		{
			get
			{
				if (_squarePositionFromZeroToOne == null)
				{
					_squarePositionFromZeroToOne = CreatePrimitivePosition(_squareFromZeroToOne, _squareIndices);
				}

				return _squarePositionFromZeroToOne;
			}
		}

		public static MeshData SquarePositionTextureFromZeroToOne
		{
			get
			{
				if (_squarePositionTextureFromZeroToOne == null)
				{
					_squarePositionTextureFromZeroToOne = new MeshData(_squarePositionTextureFromZeroToOneData, _squareIndices);
				}

				return _squarePositionTextureFromZeroToOne;
			}
		}

		private static MeshData CreatePrimitivePosition(Vector3[] positions, short[] indices)
		{
			var vertices = new List<VertexPosition>();
			foreach (var point in positions)
			{
				vertices.Add(new VertexPosition(point));
			}

			return new MeshData(vertices.ToArray(), indices);
		}
	}
}