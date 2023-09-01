using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nursia.Vertices;

namespace Nursia.Graphics3D
{
	internal static class PrimitiveMeshes
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

		private static Vector3[] _cubePoints = new Vector3[]
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

		private static Vector3[] _squarePoints = new Vector3[]
		{
				new Vector3(-1, 0, -1),
				new Vector3(-1, 0, 1),
				new Vector3(1, 0, -1),
				new Vector3(1, 0, -1),
				new Vector3(-1, 0, 1),
				new Vector3(1, 0, 1)
		};

		private static MeshData _cubePosition;
		private static MeshData _squarePosition;

		public static MeshData CubePosition
		{
			get
			{
				if (_cubePosition == null)
				{
					var vertices = new List<VertexPosition>();
					foreach (var point in _cubePoints)
					{
						vertices.Add(new VertexPosition(point));
					}

					_cubePosition = CreatePrimitivePosition(_cubePoints, _cubeIndices);
				}

				return _cubePosition;
			}
		}
		public static MeshData SquarePosition
		{
			get
			{
				if (_squarePosition == null)
				{
					var vertices = new List<VertexPosition>();
					foreach (var point in _squarePoints)
					{
						vertices.Add(new VertexPosition(point));
					}

					_squarePosition = CreatePrimitivePosition(_squarePoints, _squareIndices);
				}

				return _squarePosition;
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