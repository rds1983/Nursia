using Microsoft.Xna.Framework;
using Nursia.Rendering;
using Nursia.Utilities;
using Nursia.Data.Meshes;

namespace Nursia.Primitives
{
	public class Plane : PrimitiveMeshNode
	{
		private Vector2 _size = Vector2.One;
		private Point _tessellation = new Point(1, 1);
		private bool _generateBackFace;
		private NormalDirection _normalDirection;

		public Vector2 Size
		{
			get => _size;

			set
			{
				if (value.EpsilonEquals(_size))
				{
					return;
				}

				_size = value;
				InvalidateMesh();
			}
		}

		public Point Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		public bool GenerateBackface
		{
			get => _generateBackFace;

			set
			{
				if (value == _generateBackFace)
				{
					return;
				}

				_generateBackFace = value;
				InvalidateMesh();
			}
		}

		public NormalDirection NormalDirection
		{
			get => _normalDirection;

			set
			{
				if (value == _normalDirection)
				{
					return;
				}

				_normalDirection = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh() => MeshHelper.CreatePlane(Size.X, Size.Y, Tessellation.X, Tessellation.Y, UScale, VScale, GenerateBackface, IsLeftHanded, NormalDirection);
	}
}