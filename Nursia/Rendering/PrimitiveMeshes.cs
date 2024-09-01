using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VertexPosition = Nursia.Rendering.Vertices.VertexPosition;

namespace Nursia.Rendering
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

		private static Mesh _cubePosition;
		private static Mesh _squarePositionFromZeroToOne;
		private static Mesh _squarePositionTextureFromZeroToOne;

		public static Mesh CubePositionFromMinusOneToOne
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

		public static Mesh CubePositionFromZeroToOne
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

		public static Mesh SquarePositionFromMinusOneToOne
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

		public static Mesh SquarePositionFromZeroToOne
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

		public static Mesh SquarePositionTextureFromZeroToOne
		{
			get
			{
				if (_squarePositionTextureFromZeroToOne == null)
				{
					_squarePositionTextureFromZeroToOne = new Mesh(_squarePositionTextureFromZeroToOneData, _squareIndices);
				}

				return _squarePositionTextureFromZeroToOne;
			}
		}

		private static Mesh CreatePrimitivePosition(Vector3[] positions, short[] indices)
		{
			var vertices = new List<VertexPosition>();
			foreach (var point in positions)
			{
				vertices.Add(new VertexPosition(point));
			}

			return new Mesh(vertices.ToArray(), indices);
		}
	}
}